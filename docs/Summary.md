# Diagnostics and Code Refactoring
In this document, you can find the list of diagnostics and code refactorings supported by Acuminator.

## Diagnostics

Acuminator diagnostics are displayed if the value of **Tools > Options > Acuminator > Code Analysis > Enable code analysis** is _True_ (which is the default setting).

Acuminator does not perform static analysis of projects whose names contain `Test` or `Benchmark`. Generally, these projects contain tests of an application and do not include any application logic; therefore there is no need to analyze them with Acuminator.

**Note:** In the following table, the types of the diagnostics are specified for the ISV solution certification&#8212;that is, if the **Enable additional diagnostics for ISV Solution Certification** option (in **Tools > Options > Acuminator > Code Analysis**) is set to `True`. The type of the diagnostic can be different if this option is set to `False`. For details about the type of the diagnostic, see the description of the diagnostic.

| Code   | Short Description                                       | Type  | Code Fix  |
| ------ | ------------------------------------------------------- | ----- | --------- |
| [PX1000](diagnostics/PX1000.md) | An invalid signature of the `PXAction` handler is used. | Error | Available |
| [PX1001](diagnostics/PX1001.md) | A `PXGraph` instance must be created with the `PXGraph.CreateInstance()` factory method. | Error | Available |
| [PX1002](diagnostics/PX1002.md) | The field must have a type attribute that corresponds to the list attribute. | Error | Available |
| [PX1003](diagnostics/PX1003.md) | Consider using a specific implementation of `PXGraph`. | Warning (ISV Level 2: Production Quality) | Unavailable |
| [PX1004](diagnostics/PX1004.md) | The order of view declarations will cause the creation of two cache instances. | Message | Unavailable |
| [PX1005](diagnostics/PX1005.md) | There is probably a typo in the view delegate name. | Warning (ISV Level 3: Informational) | Available |
| [PX1006](diagnostics/PX1006.md) | The order of view declarations will cause the creation of one cache instance for multiple DACs | Message | Unavailable |
| [PX1007](diagnostics/PX1007.md) | A public entity or DAC property should have a description in the `summary` XML tag. | Warning | Available |
| [PX1008](diagnostics/PX1008.md) | The reference of `@this` graph in the delegate will cause synchronous delegate execution. | Warning (ISV Level 1: Significant) | Unavailable |
| [PX1009](diagnostics/PX1009.md) | Multiple levels of inheritance are not supported for `PXCacheExtension`. | Error | Available |
| [PX1010](diagnostics/PX1010.md) | If a delegate applies paging in an inner select, `StartRow` must be reset. (If `StartRow` is not reset, paging will be applied twice.) | Warning (ISV Level 1: Significant) | Available |
| [PX1011](diagnostics/PX1011.md) | Because multiple levels of inheritance are not supported for `PXCacheExtension`, the derived type can be marked as sealed. | Warning (ISV Level 3: Informational) | Available |
| [PX1012](diagnostics/PX1012.md) | `PXAction` is declared on a non-primary view. | Warning (ISV Level 2: Production Quality) | Available |
| [PX1013](diagnostics/PX1013.md) | The action handler that initiates a background operation or is executed by a background operation must return `IEnumerable`. | Error | Available   | 
| [PX1014](diagnostics/PX1014.md) | A DAC field must have a nullable type. | Error   | Available |
| [PX1015](diagnostics/PX1015.md) | For a BQL statement that contains parameters, the number of arguments of a `Select` method is different from the number of parameters. | Warning (ISV Level 1: Significant) | Unavailable |
| [PX1018](diagnostics/PX1018.md) | The graph with the specified primary view type parameter doesn't contain the primary view of the specified type. | Error | Unavailable |
| [PX1021](diagnostics/PX1021.md) | The type of the DAC field attribute does not correspond to the property type. | Error | Available |
| [PX1023](diagnostics/PX1023.md) | The DAC property is marked with multiple field type attributes. | Error | Available |
| [PX1024](diagnostics/PX1024.md) | The DAC class field must be abstract. | Error | Available |
| [PX1026](diagnostics/PX1026.md) | Underscores cannot be used in the names of DACs and DAC fields. | Error | Available |
| [PX1027](diagnostics/PX1027.md) | The `CompanyMask`, `CompanyID`, and `DeletedDatabaseRecord` fields cannot be declared in DACs. | Error | Available |
| [PX1028](diagnostics/PX1028.md) | Constructors in DACs are prohibited. | Error | Available |
| [PX1029](diagnostics/PX1029.md) | `PXGraph` instances cannot be used inside DAC properties. | Error | Unavailable |
| [PX1030](diagnostics/PX1030.md) | The `PXDefault` attribute of the field is used incorrectly. | Warning (ISV Level 1: Significant) or Error | Available |
| [PX1031](diagnostics/PX1031.md) | DACs cannot contain instance methods. | Error | Unavailable |
| [PX1032](diagnostics/PX1032.md) | DAC properties cannot contain method invocations. | Error | Unavailable |
| [PX1040](diagnostics/PX1040.md) | Instance constructors in BLC extensions are strictly prohibited. You should use the `Initialize()` method instead. | Error | Available |
| [PX1042](diagnostics/PX1042.md) | In a `RowSelecting` handler, BQL statements and other database queries must be executed only inside a separate connection scope. | Error | Available |
| [PX1043](diagnostics/PX1043.md) | Changes cannot be saved to the database from event handlers. | Error | Unavailable |
| [PX1044](diagnostics/PX1044.md) | Changes to `PXCache` cannot be performed in event handlers. | Error | Unavailable |
| [PX1045](diagnostics/PX1045.md) | `PXGraph` instances cannot be created in event handlers. | Error | Unavailable |
| [PX1046](diagnostics/PX1046.md) | Long-running operations cannot be started within event handlers. | Error | Unavailable |
| [PX1047](diagnostics/PX1047.md) | In the `FieldDefaulting`, `FieldVerifying`, and `RowSelected` event handlers, DAC instances passed to these event handlers cannot be modified. | Error | Unavailable |
| [PX1048](diagnostics/PX1048.md) | For the `RowInserting` and `RowSelecting` events, only the DAC instance that is passed in the event arguments can be modified in the event handler. | Error | Unavailable |
| [PX1049](diagnostics/PX1049.md) | In `RowSelected` event handlers, BQL statements and other database queries should be avoided. | Warning (ISV Level 1: Significant) | Unavailable |
| [PX1050](diagnostics/PX1050.md) | Hardcoded strings cannot be used as parameters for localization methods and `PXException` constructors. | Error | Unavailable |
| [PX1051](diagnostics/PX1051.md) | The strings defined in a class without the `PXLocalizable` attribute cannot be used as parameters for localization methods and `PXException` constructors. | Error | Unavailable |
| [PX1052](diagnostics/PX1052.md) | Plain text strings cannot be used in the methods of the `LocalizeFormat` family. | Error | Unavailable |
| [PX1053](diagnostics/PX1053.md) | Concatenated strings cannot be used as parameters for localization methods and `PXException` constructors. | Error | Unavailable |
| [PX1054](diagnostics/PX1054.md) | A `PXGraph` instance cannot start a long-running operation during the `PXGraph` initialization. | Error | Unavailable |
| [PX1055](diagnostics/PX1055.md) | An invalid primary key of the DAC is used. | Error | Available |
| [PX1057](diagnostics/PX1057.md) | A `PXGraph` instance cannot be initialized while another `PXGraph` instance is being initialized. | Error | Unavailable |
| [PX1058](diagnostics/PX1058.md) | A `PXGraph` instance cannot save changes to the database during the `PXGraph` initialization. | Error | Unavailable |
| [PX1059](diagnostics/PX1059.md) | Changes to `PXCache` cannot be performed during the `PXGraph` initialization. | Error | Unavailable |
| [PX1060](diagnostics/PX1060.md) | DAC fields should be strongly typed to be used in fluent BQL queries. | Message | Available |
| [PX1061](diagnostics/PX1061.md) | Constants should be strongly typed to be used in fluent BQL queries. | Message | Available |
| [PX1070](diagnostics/PX1070.md) | The state of fields and actions can be configured only in `RowSelected` event handlers. | Error | Unavailable |
| [PX1071](diagnostics/PX1071.md) | Actions cannot be executed within event handlers. | Error | Unavailable |
| [PX1072](diagnostics/PX1072.md) | BQL queries must be executed within the context of an existing `PXGraph` instance. | Warning (ISV Level 1: Significant) | Available |
| [PX1073](diagnostics/PX1073.md) | Exceptions cannot be thrown in the `RowPersisted` event handlers. | Error | Unavailable |
| [PX1074](diagnostics/PX1074.md) | `PXSetupNotEnteredException` cannot be thrown in any event handlers except for the `RowSelected` event handlers. | Warning (ISV Level 1: Significant) | Unavailable |
| [PX1075](diagnostics/PX1075.md) | `PXCache.RaiseExceptionHandling` cannot be invoked from the `FieldDefaulting`, `FieldSelecting`, `RowSelecting`, and `RowPersisted` event handlers. | Error | Unavailable |
| [PX1080](diagnostics/PX1080.md) | Data view delegates should not start long-running operations. | Error | Unavailable |
| [PX1081](diagnostics/PX1081.md) | Actions cannot be executed during the `PXGraph` initialization. | Error | Unavailable |
| [PX1082](diagnostics/PX1082.md) | Actions cannot be executed within data view delegates. | Error | Unavailable |
| [PX1083](diagnostics/PX1083.md) | Changes cannot be saved to the database from data view delegates. | Error | Unavailable |
| [PX1084](diagnostics/PX1084.md) | `PXGraph` instances cannot be initialized within data view delegates. | Error | Unavailable |
| [PX1085](diagnostics/PX1085.md) | BQL statements and other database queries should not be executed during the `PXGraph` initialization. | Warning (ISV Level 1: Significant) | Unavailable |
| [PX1086](diagnostics/PX1086.md) | `PXSetupNotEnteredException` cannot be thrown in long-running operations. | Warning (ISV Level 1: Significant) | Unavailable |
| [PX1087](diagnostics/PX1087.md) | This invocation of the base data view delegate can cause a `StackOverflowException`. | Warning (ISV Level 1: Significant) | Unavailable |
| [PX1088](diagnostics/PX1088.md) | Processing delegates cannot use the data views from processing graphs, except for the data views of the `PXFilter`, `PXProcessingBase`, and `PXSetup` types. | Warning (ISV Level 2: Production Quality) | Unavailable |
| [PX1089](diagnostics/PX1089.md) | The state of fields and actions cannot be configured in the action delegates. | Error | Unavailable |
| [PX1090](diagnostics/PX1090.md) | `PXSetupNotEnteredException` cannot be thrown in action handlers. | Warning (ISV Level 1: Significant) | Unavailable |
| [PX1091](diagnostics/PX1091.md) | This invocation of the base action handler can cause a `StackOverflowException`. | Warning (ISV Level 1: Significant) | Unavailable |
| [PX1092](diagnostics/PX1092.md) | Action handlers must be decorated with the `PXUIField` attribute and with the `PXButton` attribute or its successors. | Error | Available |
| [PX1093](diagnostics/PX1093.md) | In a graph declaration, the first type parameter of `PXGraph` must be the graph type. | Error | Available |
| [PX1094](diagnostics/PX1094.md) | Every DAC should be decorated with the `PXHidden` or `PXCacheName` attribute. | Warning (ISV Level 3: Informational) | Available |
| [PX1095](diagnostics/PX1095.md) | A field with the `PXDBCalced` attribute must have an unbound type attribute, such as `PXDate` or `PXDecimal`. | Error | Unavailable |

## Code Refactoring
Acuminator suggest the following code refactoring:
 - [Replacement of the Standard Event Handler Signature with the Generic Signature](refactoring/GenericEventHandlerSignature.md) |

