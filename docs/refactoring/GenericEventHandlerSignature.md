# Replacement of the Standard Event Handler Signature with the Generic Signature
Acuminator suggests replacement of the standard event handler signature with the generic signature. The following code example shows the standard and generic event handler signatures.

```C#
protected virtual void ARInvoice_RefNbr_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e) // Standard signature
protected virtual void _(Events.FieldUpdating<ARInvoice, ARInvoice.refNbr> e) // Generic signature
```

Because an event handler can be overridden in derived classes or graph extensions, after you have applied this refactoring to your code, you have to manually update all possible overrides.

