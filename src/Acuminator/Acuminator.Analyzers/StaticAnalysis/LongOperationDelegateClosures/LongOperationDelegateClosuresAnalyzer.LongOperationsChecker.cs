#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.ArgumentsToParametersMapping;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Utilities.Roslyn.Walkers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	public partial class LongOperationDelegateClosuresAnalyzer : PXDiagnosticAnalyzer
	{
		private class LongOperationsChecker : NestedInvocationWalker
		{
			private readonly SyntaxNodeAnalysisContext _context;
			private readonly ParametersReassignedFinder _parametersReassignedFinder = new();
			private readonly NonCapturableElementsInArgumentsFinder _nonCapturableElementsInArgumentsFinder;

			private Stack<PassedParametersToNotBeCaptured?> NonCapturablePassedParameters { get; set;  } = new();

			public LongOperationsChecker(SyntaxNodeAnalysisContext context, PXContext pxContext) :
									base(pxContext, context.CancellationToken)
			{
				_context = context;
				_nonCapturableElementsInArgumentsFinder = new NonCapturableElementsInArgumentsFinder(CancellationToken, PxContext);
			}

			public void CheckForCapturedGraphReferencesInDelegateClosures(TypeDeclarationSyntax typeDeclarationNode)
			{
				ThrowIfCancellationRequested();

				typeDeclarationNode.Accept(this);
			}

			#region Visitor Optimization - do not visit some subtrees
			public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node) { }

			public override void VisitEnumDeclaration(EnumDeclarationSyntax node) { }

			public override void VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node) { }

			public override void VisitXmlComment(XmlCommentSyntax node) { }
			#endregion

			public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax localFunctionDeclaration)
			{
				// There are two ways to get into local function statement with the nested invocation walker:
				// 1. Visit it during the recursive visit of a local function call. In such case it can be processesd as usual
				// 2. Visit the declaration during the normal syntax walking.
				
				if (localFunctionDeclaration.Equals(NodeCurrentlyVisitedRecursively))
				{
					base.VisitLocalFunctionStatement(localFunctionDeclaration);
					return;
				}

				// When we visit local function declaration during the syntax walking it is like we start visiting another method at the top of call stack
				// No previous recursive context applies to the local function declaration itself because it is a declaration, not a call.
				// Thus, we need to save the NonCapturablePassedParameters stack, reset it and then restore.
				// 
				// Also, we use dynamic programming approach to calculate all parameters available to a local function from containing methods recurrently.
				// This means that we usually need the parameters in the NonCapturablePassedParameters stack.
				// But in this case it is OK because all containing methods' parameters available to the local function in this case should be considered capturable 
				// since we don't have any recursive analysis context.
				// There is only one exception - the PXAdapter parameter from action delegates. It is obtained through an independent calculation.
				// The analysis can check for the capturing of the adapter and also for capturing this reference in graphs and graph extensions. 
				var oldNonCapturablePassedParameters = NonCapturablePassedParameters;

				try
				{
					NonCapturablePassedParameters = new Stack<PassedParametersToNotBeCaptured?>();
					base.VisitLocalFunctionStatement(localFunctionDeclaration);
				}
				finally
				{
					NonCapturablePassedParameters = oldNonCapturablePassedParameters;
				}
			}

			#region Visiting invocations and checking long run delegates 
			public override void VisitInvocationExpression(InvocationExpressionSyntax invocationExpression)
			{
				ThrowIfCancellationRequested();

				SemanticModel? semanticModel = GetSemanticModel(invocationExpression.SyntaxTree);

				if (semanticModel == null)
				{
					base.VisitInvocationExpression(invocationExpression);
					return;
				}

				var longOperationDelegateType = LongOperationDelegateTypeClassifier.GetLongOperationDelegateType(invocationExpression,
																												 semanticModel, PxContext, CancellationToken);
				switch (longOperationDelegateType)
				{
					case LongOperationDelegateType.ProcessingDelegate:
						AnalyzeSetProcessDelegateMethod(semanticModel, invocationExpression);
						return;

					case LongOperationDelegateType.LongRunDelegate:
						AnalyzeStartOperationDelegateMethod(semanticModel, invocationExpression);
						return;

					default:
						base.VisitInvocationExpression(invocationExpression);
						return;
				}
			}

			private void AnalyzeSetProcessDelegateMethod(SemanticModel semanticModel, InvocationExpressionSyntax setDelegateInvocation)
			{
				if (setDelegateInvocation.ArgumentList.Arguments.Count == 0)
					return;

				ExpressionSyntax processingDelegateParameter = setDelegateInvocation.ArgumentList.Arguments[0].Expression;
				bool isMainProcessingDelegateCorrect = CheckDataFlowForDelegateMethod(semanticModel, setDelegateInvocation, processingDelegateParameter,
																					  LongOperationDelegateType.ProcessingDelegate);

				if (isMainProcessingDelegateCorrect && setDelegateInvocation.ArgumentList.Arguments.Count > 1)
				{
					ExpressionSyntax finallyHandlerDelegateParameter = setDelegateInvocation.ArgumentList.Arguments[1].Expression;
					CheckDataFlowForDelegateMethod(semanticModel, setDelegateInvocation, finallyHandlerDelegateParameter,
												   LongOperationDelegateType.ProcessingDelegate);
				}
			}

			private void AnalyzeStartOperationDelegateMethod(SemanticModel semanticModel, InvocationExpressionSyntax startOperationInvocation)
			{
				if (startOperationInvocation.ArgumentList.Arguments.Count < 2)
					return;

				ExpressionSyntax longRunDelegateParameter = startOperationInvocation.ArgumentList.Arguments[1].Expression;
				CheckDataFlowForDelegateMethod(semanticModel, startOperationInvocation, longRunDelegateParameter,
											   LongOperationDelegateType.LongRunDelegate);
			}

			private bool CheckDataFlowForDelegateMethod(SemanticModel semanticModel, InvocationExpressionSyntax longOperationSetupMethodInvocationNode,
														ExpressionSyntax longOperationDelegateNode, LongOperationDelegateType delegateType)
			{
				ThrowIfCancellationRequested();

				PassedParametersToNotBeCaptured? nonCapturableParametersOfMethodContainingCallSite =
					GetNonCapturableParametersForDelegateMethodWithReassignmentCheck(semanticModel, longOperationSetupMethodInvocationNode);

				var capturedLocalInstancesInExpressionsChecker =
					new CapturedLocalInstancesInExpressionsChecker(nonCapturableParametersOfMethodContainingCallSite, semanticModel, PxContext,
																   CancellationToken);
				var capturedInstanceType = capturedLocalInstancesInExpressionsChecker.ExpressionCapturesLocalIntanceInClosure(longOperationDelegateNode);

				if (capturedInstanceType != CapturedInstancesTypes.None)
				{
					string capturedInstanceTypeName = (capturedInstanceType & CapturedInstancesTypes.PXGraph) == CapturedInstancesTypes.PXGraph
						? Resources.PX1008Title_CapturedGraphFormatArg
						: Resources.PX1008Title_CapturedPXAdapterFormatArg;

					string delegateTypeName = delegateType == LongOperationDelegateType.ProcessingDelegate
						? Resources.PX1008Title_ProcessingDelegateFormatArg
						: Resources.PX1008Title_LongRunDelegateFormatArg;

					ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1008_LongOperationDelegateClosures, longOperationSetupMethodInvocationNode,
									 capturedInstanceTypeName, delegateTypeName);
					return false;
				}

				return true;
			}

			private PassedParametersToNotBeCaptured? GetNonCapturableParametersForDelegateMethodWithReassignmentCheck(SemanticModel semanticModel,
																							InvocationExpressionSyntax longOperationSetupMethodInvocationNode)
			{			
				if (IsInsideRecursiveCall)     // if we inside the call stack then we need to check captured elements for reassignment
				{
					PassedParametersToNotBeCaptured? nonCapturableParametersOfMethodContainingCallSite = PeekPassedParametersFromStack();

					if (nonCapturableParametersOfMethodContainingCallSite == null)
						return null;

					var callingMemberNode = longOperationSetupMethodInvocationNode.Parent<MemberDeclarationSyntax>();

					if (callingMemberNode == null)
						return null;

					// Check if captured parameters were reassigned
					var reassignedParameters = GetReassignedParametersNames(callingMemberNode, nonCapturableParametersOfMethodContainingCallSite.PassedParametersNames);

					if (reassignedParameters.IsNullOrEmpty())
						return nonCapturableParametersOfMethodContainingCallSite;

					var notReassignedParameters = nonCapturableParametersOfMethodContainingCallSite.PassedParameters
																								   .Where(p => !reassignedParameters.Contains(p.Name));
					return new PassedParametersToNotBeCaptured(notReassignedParameters);
				}
				else                           // if we are at the top of call stack then we need to look for adapter parameter in case we are in action handler
				{
					var callingMethodNode = longOperationSetupMethodInvocationNode.Parent<MethodDeclarationSyntax>();

					if (callingMethodNode == null)
						return null;

					var callingMethod = semanticModel.GetDeclaredSymbol(callingMethodNode, CancellationToken);
					var adapterParameter = GetNonCapturableAdapterParameter(callingMethod);

					if (adapterParameter == null)
						return null;

					// Check if adapter was reassigned
					var reassignedParameters = GetReassignedParametersNames(callingMethodNode, new List<string>(capacity: 1) { adapterParameter.Name });

					if (reassignedParameters?.Count > 0)
						return null;

					return new PassedParametersToNotBeCaptured
					(
						new List<PassedParameter>(capacity: 1)
						{
							new PassedParameter(adapterParameter.Name, CapturedInstancesTypes.PXAdapter)
						}
					);
				}

				//---------------------------------------Local Function-----------------------------------------------------------------------
				HashSet<string>? GetReassignedParametersNames(MemberDeclarationSyntax callingMemberName, IReadOnlyCollection<string> parametersNames) =>
					_parametersReassignedFinder.GetParametersReassignedBeforeCallsite(callingMemberName, longOperationSetupMethodInvocationNode,
																					  parametersNames, semanticModel, CancellationToken);
			} 
			#endregion

			#region Recursive method calls visiting - filtering visited methods and perform data flow analysis
			protected override void BeforeBypassCheck(IMethodSymbol calledMethod, CSharpSyntaxNode calledMethodNode, ExpressionSyntax callSite)
			{
				base.BeforeBypassCheck(calledMethod, calledMethodNode, callSite);

				PassedParametersToNotBeCaptured? nonCapturableParameters = GetMethodNonCapturableParameters(calledMethod, callSite);

				NonCapturablePassedParameters.Push(nonCapturableParameters);
			}

			protected override bool BypassMethod(IMethodSymbol calledMethod, CSharpSyntaxNode calledMethodNode, ExpressionSyntax callSite)
			{
				if (base.BypassMethod(calledMethod, calledMethodNode, callSite))
					return true;

				PassedParametersToNotBeCaptured? calledMethodNonCapturableParameters = PeekPassedParametersFromStack();

				// If the called method has some non-capturable method parameters then we need to step into it
				if (calledMethodNonCapturableParameters?.Count > 0)
					return false;

				// Otherwise, we need to step only into non-static methods of graphs/graph extensions since they can capture "this" reference
				return calledMethod.IsStatic || calledMethod.ContainingType == null || !calledMethod.ContainingType.IsPXGraphOrExtension(PxContext);
			}

			protected override void AfterRecursiveVisit(IMethodSymbol calledMethod, CSharpSyntaxNode calledMethodNode, ExpressionSyntax callSite, bool wasVisited)
			{
				base.AfterRecursiveVisit(calledMethod, calledMethodNode, callSite, wasVisited);

				NonCapturablePassedParameters.Pop();
			}

			private PassedParametersToNotBeCaptured? GetMethodNonCapturableParameters(IMethodSymbol calledMethod, ExpressionSyntax callSite)
			{
				if (!CanCalledMethodCaptureNonCapturableElements(calledMethod))
					return null;

				var argumentsList = callSite.GetArgumentsList();

				if (argumentsList == null || (argumentsList.Arguments.Count == 0 && calledMethod.MethodKind != MethodKind.LocalFunction))
					return null;

				var callingTypeMemberNode = callSite.Parent<MemberDeclarationSyntax>();

				if (callingTypeMemberNode == null)
					return null;

				SemanticModel? semanticModel = GetSemanticModel(callSite.SyntaxTree);
				ISymbol? callingTypeMember = semanticModel?.GetDeclaredSymbol(callingTypeMemberNode, CancellationToken);

				if (callingTypeMember?.ContainingType == null)
					return null;

				var nonCapturableParameters = GetNonCapturableParameters(callingTypeMember, callingTypeMemberNode, argumentsList, 
																		 callSite, semanticModel!, calledMethod);
				if (nonCapturableParameters.IsNullOrEmpty())
					return null;

				return new PassedParametersToNotBeCaptured(nonCapturableParameters);
			}

			private bool CanCalledMethodCaptureNonCapturableElements(IMethodSymbol calledMethod)
			{
				// If the method has parameters it can capture some non capturable elements in them
				if (!calledMethod.Parameters.IsDefaultOrEmpty)
					return true;

				// If the method doesn't have parameters it can capture this reference unless it is a static method
				if (calledMethod.IsStatic)
					return false;

				bool isInsideGraph = calledMethod.ContainingType?.IsPXGraphOrExtension(PxContext) ?? false;

				//Method can capture non-capturable this reference only if it is inside graph or graph extension
				if (isInsideGraph)
					return true;

				// If the method is a non static local function it can also capture parameters from the outer method
				if (calledMethod.MethodKind == MethodKind.LocalFunction && calledMethod.ContainingSymbol is IMethodSymbol containingMethod)
					return !containingMethod.Parameters.IsDefaultOrEmpty;
				
				return false;
			}

			private IReadOnlyCollection<PassedParameter>? GetNonCapturableParameters(ISymbol callingTypeMember, MemberDeclarationSyntax callingTypeMemberNode,
																					 BaseArgumentListSyntax argumentsList, ExpressionSyntax callSite, 
																					 SemanticModel semanticModel, IMethodSymbol calledMethod)
			{
				ThrowIfCancellationRequested();

				// There can be four types of non capturable parameters that can be passed to other methods:
				// 1. PXAdapter from action delegate if we are at the top of call stack.
				// 2. this reference if we are in a graph or graph extension
				// 3. non capturable parameters passed to the calling methods from the previous method call		
				// 4. for a non-static local function parameters from all containing methods up to the non-local method or first static local function.
				 
				// Search for the adapter only if we are at the top of call stack
				IParameterSymbol? adapterParameter = IsInsideRecursiveCall
					? null
					: GetNonCapturableAdapterParameter(callingTypeMember as IMethodSymbol);

				NonCapturableElementsInfo? nonCapturableElements = GetNonCapturableElementsInfo(argumentsList.Arguments, callingTypeMember, 
																								adapterParameter, calledMethod);
				if (nonCapturableElements == null || !nonCapturableElements.HasNonCapturableElements)
					return null;

				// We also can filter for reassigned parameters
				FilterReassignedParameters(nonCapturableElements, callingTypeMemberNode, callSite, semanticModel);

				if (!nonCapturableElements.HasNonCapturableElements)
					return null;

				var nonCapturableParametersFromCallArguments = 
					GetNonCapturableParametersFromCallArguments(calledMethod, nonCapturableElements, argumentsList);

				if (!nonCapturableElements.HasNonCapturableContainingMethodsParameters)
					return nonCapturableParametersFromCallArguments;

				var notRedefinedContainingMethodParameters = GetContainingMethodParametersNotRedefinedByCalledMethod();

				if (notRedefinedContainingMethodParameters.Count == 0)
					return nonCapturableParametersFromCallArguments;
				else if (nonCapturableParametersFromCallArguments.IsNullOrEmpty())
					return notRedefinedContainingMethodParameters;

				nonCapturableParametersFromCallArguments.AddRange(notRedefinedContainingMethodParameters);
				return nonCapturableParametersFromCallArguments;

				//-----------------------------------------------Local Function-----------------------------------------
				IReadOnlyCollection<PassedParameter> GetContainingMethodParametersNotRedefinedByCalledMethod()
				{
					if (calledMethod.Parameters.IsDefaultOrEmpty)
						return nonCapturableElements!.NonCapturableContainingMethodsParameters!;
					else
					{
						return (from containingMethodParameter in nonCapturableElements!.NonCapturableContainingMethodsParameters
								where !calledMethod.Parameters.Any(p => p.Name == containingMethodParameter.Name)
								select containingMethodParameter)
							 .ToList(capacity: nonCapturableElements.NonCapturableContainingMethodsParameters!.Count);
					}
				}
			}

			/// <summary>
			/// Attempts to get a non-capturable adapter parameter from the <paramref name="callingMethod"/>
			/// if the calling method is an action handler with an adapter parameter.
			/// </summary>
			/// <remarks>
			/// This method checks the calling method to see if it could be an action handler with PXAdapter adapter parameter.
			/// Adapters from action handlers can't be captured by long run delegates' closures.
			/// <br/><br/>
			/// When we check the method to see if it can be an action handler we must take into account that actions can be added to graph dynamically.
			/// This means that graphs and graph extensions are not only places declaring action handlers.<br/>
			/// For example, actions can be added in custom attributes and views. Thus, we can't make any assumptions about the method's containing type.<br/>
			/// Therefore, we will consider a method to be an action handler if it has a proper signature and a PXButton attribute declared on it or on base methods in case of overrides.
			/// </remarks>
			/// <param name="callingMethod">The method containing the call.</param>
			/// <returns>
			/// The adapter parameter or null.
			/// </returns>
			private IParameterSymbol? GetNonCapturableAdapterParameter(IMethodSymbol? callingMethod)
			{
				if (callingMethod?.ContainingType == null)
					return null;

				var actionHandlerMethod = callingMethod.MethodKind != MethodKind.LocalFunction
					? callingMethod
					: callingMethod.GetStaticOrNonLocalContainingMethod();

				// When we check local function declared we try to get the non local containing method OR first containing static local function
				// If we obtain the former than our local function can potentially use adapter parameter from the action handler.
				// Otherwise, in case of a static local function it will be unavailable and we can return
				if (actionHandlerMethod == null || actionHandlerMethod.MethodKind == MethodKind.LocalFunction)
					return null;

				if (actionHandlerMethod.Parameters.IsDefaultOrEmpty)
					return null;

				// Obtain adapter parameter
				var (adapterParameter, hasPXButtonAttribute) = FindAdapterParameterInMethod(actionHandlerMethod);

				if (adapterParameter == null)
					return null;
				
				// Check if parameter is redefined by local functions
				if (callingMethod.MethodKind == MethodKind.LocalFunction && callingMethod.IsNonLocalMethodParameterRedefined(adapterParameter.Name))
					return null;

				// Check for PXButton attribute - if method has it then it is an action handler
				if (hasPXButtonAttribute)
					return adapterParameter;

				// Check for action handlers that are declared inside the current graph but are not marked with attribute
				INamedTypeSymbol containingType = actionHandlerMethod.ContainingType;
				bool isInsideGraph              = containingType.IsPXGraph(PxContext);
				bool isInsideGraphExtension     = containingType.IsPXGraphExtension(PxContext);

				// If method is outside graph or graph extension then assume that it's not an action handler
				if (!isInsideGraph && !isInsideGraphExtension)
					return null;

				IEnumerable<ITypeSymbol> typesToCheck = isInsideGraph
					? containingType.GetGraphWithBaseTypes()
					: containingType.GetGraphExtensionWithBaseExtensions(PxContext, SortDirection.Descending, includeGraph: true);

				bool hasCorrespondingAction = typesToCheck.SelectMany(type => type.GetMembers().OfType<IFieldSymbol>())
														  .Any(field => field.Name.Equals(actionHandlerMethod.Name, StringComparison.OrdinalIgnoreCase) &&
																		field.Type.IsPXAction(PxContext));	
				return hasCorrespondingAction
					? adapterParameter
					: null;
			}

			

			private (IParameterSymbol? AdapterParameter, bool HasPXButtonAttribute) FindAdapterParameterInMethod(IMethodSymbol actionHandlerMethod)
			{
				// Check if method is valid graph action handler
				if (actionHandlerMethod.IsValidActionHandler(PxContext))
					return (actionHandlerMethod.Parameters[0], HasPXButtonAttribute(actionHandlerMethod));

				// if method is not valid graph action handler then check for PXButton attribute. 
				// Sometimes it is declared on action handlers with custom signatures that are used by dynamically added actions
				if (!HasPXButtonAttribute(actionHandlerMethod))
					return (AdapterParameter: null, HasPXButtonAttribute: false);   // If there is no PXButton attribute then it's not an action handler

				// If the method has PXButton attribute then do more flexible search for adapter parameter.
				// Check if the method has a single adapter parameter and if yes then return it
				var adapterParameters = actionHandlerMethod.Parameters.Where(parameter => parameter.Type.Equals(PxContext.PXAdapterType))
																	  .ToList(capacity: 1);
				return adapterParameters.Count == 1
					? (AdapterParameter: adapterParameters[0], HasPXButtonAttribute: true)
					: (AdapterParameter: null, HasPXButtonAttribute: true);
			}

			private bool HasPXButtonAttribute(IMethodSymbol method) => 
				method.HasAttribute(PxContext.AttributeTypes.PXButtonAttribute, checkOverrides: true);

			private NonCapturableArgumentsInfo? GetNonCapturableArgumentsInfo(SeparatedSyntaxList<ArgumentSyntax> callArguments, ISymbol callingTypeMember, 
																			  IParameterSymbol? adapterParameter)
			{
				var parametersPassedBefore = GetAllPassedParametersByNames(adapterParameter?.Name, callingTypeMember as IMethodSymbol);

				if (parametersPassedBefore == null || parametersPassedBefore.Count == 0)
					return default;

				NonCapturableArgumentsInfo? nonCapturableArgumentsInfo = null;

				for (int argIndex = 0; argIndex < callArguments.Count; argIndex++)
				{
					ArgumentSyntax argument = callArguments[argIndex];

					var (captureLocalGraphInstance, parametersUsedInArgument) = 
						_nonCapturableElementsInArgumentsFinder.GetElementsUsedInArgumentExpression(argument, callingTypeMember, parametersPassedBefore);
					bool captureNonCapturableElement = captureLocalGraphInstance || parametersUsedInArgument?.Count > 0;

					if (captureNonCapturableElement)
					{
						nonCapturableArgumentsInfo ??= new NonCapturableArgumentsInfo();
						var nonCapturableArgument = new NonCapturableArgument(argIndex, captureLocalGraphInstance, parametersUsedInArgument);

						nonCapturableArgumentsInfo.Add(nonCapturableArgument);
					}
				}

				return nonCapturableArgumentsInfo;				
			}

			private IReadOnlyDictionary<string, PassedParameter>? GetAllPassedParametersByNames(string? adapterParameterName, IMethodSymbol? callingMethod)
			{
				var parametersPassedBefore = GetAvailablePassedParametersFromStack(callingMethod);

				if (adapterParameterName == null)
					return parametersPassedBefore;

				var passedAdapterParameter = new PassedParameter(adapterParameterName, CapturedInstancesTypes.PXAdapter);

				if (parametersPassedBefore.IsNullOrEmpty())
				{
					return new Dictionary<string, PassedParameter>(capacity: 1) 
					{ 
						{ passedAdapterParameter.Name, passedAdapterParameter } 
					};
				}
				else
				{
					var parametersByNames = new Dictionary<string, PassedParameter>(capacity: parametersPassedBefore.Count + 1)
					{
						{ passedAdapterParameter.Name, passedAdapterParameter }
					};

					foreach (var (parameterName, parameter) in parametersPassedBefore)
						parametersByNames[parameterName] = parameter; 

					return parametersByNames;
				}
			}

			private IReadOnlyDictionary<string, PassedParameter>? GetAvailablePassedParametersFromStack(IMethodSymbol? callingMethod)
			{
				if (callingMethod == null || NonCapturablePassedParameters.Count == 0)
					return null;

				var parametersPassedToCallingMethod = PeekPassedParametersFromStack();

				if (callingMethod.MethodKind != MethodKind.LocalFunction)
					return parametersPassedToCallingMethod;

				// Local functions can capture parameters from their containing methods (unless the parameter is redefined or the containing method is a static local function)
				// We need to combile all parameters from outer methods that have non-capturable elements. 
				// We could have retrieved them from the NonCapturablePassedParameters stack and combine but that would require some complex combining logic 
				// for eacn of the nested local functions (of course, it's a rare case to have local functions and its even more rare to have a local function nested in local function).
				// 
				// Instead we'll take a dynamic programming approach. 
				// We will assume that for a call located in a nested local function the NonCapturablePassedParameters stack already contains all possible combined non-capturable parameters
				// passed to the method containing this nested local function and a call to it and we only need to combine these parameters with the parameters passed to the nested local function.
				//  
				// This is a dynamic programming, when the next entry in the stack is calculated from the previous entry via a recurrent formula:
				// - For a call in local function we combine non-capturable parameters passed to it with non-capturable parameters passed to the containing non-local method
				// - For a call to a nested local function we combine non-capturable parameters passed to it with non-capturable parameters passed to its containing local function
				// - And so on.
				if (callingMethod.IsStatic)
					return parametersPassedToCallingMethod;     // no extra parameters from outer methods can be used by the static local function

				if (callingMethod.ContainingSymbol is not IMethodSymbol methodContainingCallToCallingMethod)
					return parametersPassedToCallingMethod;

				var parametersPassedToMethodContainingCallToCallingMethod = NonCapturablePassedParameters.Count > 1
					? NonCapturablePassedParameters.ElementAt(1)
					: null;

				if (parametersPassedToMethodContainingCallToCallingMethod.IsNullOrEmpty())
					return parametersPassedToCallingMethod;
				else if (parametersPassedToCallingMethod.IsNullOrEmpty())
					return parametersPassedToMethodContainingCallToCallingMethod;

				var combinedParameters = 
					new Dictionary<string, PassedParameter>(capacity: parametersPassedToCallingMethod.Count + parametersPassedToMethodContainingCallToCallingMethod.Count);

				foreach (var (parameterName, parameter) in parametersPassedToMethodContainingCallToCallingMethod)
				{
					if (!IsRedefinedByCallingMethod(parameterName))
						combinedParameters[parameterName] = parameter;
				}

				foreach (var (parameterName, parameter) in parametersPassedToCallingMethod)
					combinedParameters[parameterName] = parameter;

				return combinedParameters;

				//------------------------------------------------------------Local Function--------------------------------------------------
				bool IsRedefinedByCallingMethod(string parameterName) =>
					callingMethod!.Parameters.Any(p => p.Name == parameterName);
			}

			private void FilterReassignedParameters(NonCapturableElementsInfo nonCapturableElements, MemberDeclarationSyntax callingTypeMemberNode,
													ExpressionSyntax callSite, SemanticModel semanticModel)
			{
				if (!nonCapturableElements.HasNonCapturableParameters)
					return;

				var parameterNames = nonCapturableElements.GetNamesOfUsedNonCapturableParameters();

				if (parameterNames.IsNullOrEmpty())
					return;

				var reassignedParameters =
					_parametersReassignedFinder.GetParametersReassignedBeforeCallsite(callingTypeMemberNode, callSite, parameterNames,
																					  semanticModel, CancellationToken);
				if (reassignedParameters.IsNullOrEmpty())
					return;

				foreach (string reassignedParameter in reassignedParameters)
				{
					nonCapturableElements.RemoveParameterUsage(reassignedParameter);
				}
			}

			private List<PassedParameter>? GetNonCapturableParametersFromCallArguments(IMethodSymbol calledMethod,  NonCapturableElementsInfo nonCapturableElements,
																					   BaseArgumentListSyntax argumentsList)
			{
				if (!nonCapturableElements.HasArgumentsWithNonCapturableElements)
					return null;

				ArgumentsToParametersMapping? argumentsToParametersMapping = calledMethod.MapArgumentsToParameters(argumentsList);

				if (argumentsToParametersMapping == null)
					return null;

				var nonCapturableParametersOfCalledMethodFromCallArgs =
					new List<PassedParameter>(capacity: nonCapturableElements.ArgumentsWithNonCapturableElements.Count);

				foreach (NonCapturableArgument argument in nonCapturableElements.ArgumentsWithNonCapturableElements)
				{
					CapturedInstancesTypes capturedTypes = argument.GetCapturedTypes();

					if (capturedTypes == CapturedInstancesTypes.None)
						continue;

					var mappedParameter = argumentsToParametersMapping.Value.GetMappedParameter(calledMethod, argument.Index);
					nonCapturableParametersOfCalledMethodFromCallArgs.Add(new PassedParameter(mappedParameter.Name, capturedTypes));
				}

				return nonCapturableParametersOfCalledMethodFromCallArgs;
			}
			#endregion

			private PassedParametersToNotBeCaptured? PeekPassedParametersFromStack() => NonCapturablePassedParameters.Count > 0
					? NonCapturablePassedParameters.Peek()
					: null;
		}
	}
}