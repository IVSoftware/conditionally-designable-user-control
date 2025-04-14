First, subclass `Microsoft.DotNet.DesignTools.DesignersParentControlDesigner` in order to customize the designer behavior `UserControlEx`. 


Add a frame with the `Dock in Parent Container` verb.

[![dock in parent container](https://stackoverflowteams.com/c/sqdev/images/s/30610e78-656e-4513-9563-3c5f28df2177.png)](https://stackoverflowteams.com/c/sqdev/images/s/30610e78-656e-4513-9563-3c5f28df2177.png)

___


Next, by inspecting the value of the `ContentType` property of `UserControlEx` I can either show a `Designable` surface or an `Immutable` one. When something like a `Button` is being dragged over from the **Tool Box**, this property of the user control is inspected in the `OnDragOver` method of the custom designer and accepted in the first case and rejected in the second.


[![drag-drop disallowed](https://stackoverflowteams.com/c/sqdev/images/s/9c6c11b7-e5c4-41d6-8883-5a36468b939b.png)](https://stackoverflowteams.com/c/sqdev/images/s/9c6c11b7-e5c4-41d6-8883-5a36468b939b.png)
___