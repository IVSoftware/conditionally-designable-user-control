### Control Design is Conditionally Allowed/Disallowed 

This repo contains a prototype that is a minimal example of doing this.

___

First, subclass `Microsoft.DotNet.DesignTools.Designers.ParentControlDesigner` in order to customize the designer behavior for `UserControlEx` and set the `Designer` attribute of the user control.

```
// <PackageReference Include="Microsoft.WinForms.Designer.SDK" Version="1.6.0" />
class ParentControlDesignerEx : ParentControlDesigner{ }

[Designer(typeof(ParentControlDesignerEx))]
public partial class UserControlEx : UserControl{}
```

Optionally, add a frame with verbs for:

- `Dock in Parent Container`
- `Toggle content type`

public override DesignerVerbCollection Verbs
            => new DesignerVerbCollection(base.Verbs.Cast<DesignerVerb>().Concat(new[]
            {
                new DesignerVerb("Dock in parent container", OnDockInParent),
                new DesignerVerb("Toggle content type", OnToggleContentType),
            }).ToArray());

        void OnDockInParent(object? sender, EventArgs e)
        {
            if (Component is Control c && c.Parent != null)
            {
                c.Dock = DockStyle.Fill;
                Control?.Log($"{nameof(OnDockInParent)}");
            }
        }

        private void OnToggleContentType(object? sender, EventArgs e) =>
            Control.ContentType = Control.ContentType switch
            {
                ContentType.Immutable => ContentType.Designable,
                _ => ContentType.Immutable,
            };

        public new UserControlEx Control => (UserControlEx)base.Control;

[![dock in parent container](https://i.sstatic.net/oTRDSCaA.png)

___


Next, by inspecting the value of the `ContentType` property of `UserControlEx` it can either show a `Designable` surface or an `Immutable` one. When something like a `Button` is being dragged over from the **Tool Box**, this property of the user control is inspected in the `OnDragOver` method of the custom designer and accepted in the first case and rejected in the second.


```
public new UserControlEx Control => (UserControlEx)base.Control;
protected override void OnDragOver(DragEventArgs de)
{
    base.OnDragOver(de);

    Point pt = Control.PointToClient(new Point(de.X, de.Y));
    var target = Control.GetChildAtPoint(pt, GetChildAtPointSkip.Invisible);
    if (target is ImmutableContent)
    {
        de.Effect = DragDropEffects.None;
    }
}
protected override void OnDragDrop(DragEventArgs de)
{
    if(Control.ContentType == ContentType.Designable)
    {
        // Allow
        base.OnDragDrop(de);
    }
}
```


[![drag-drop disallowed](https://i.sstatic.net/oJKDK4WA.png)
___

What makes it tricky to suppress control addition via toolbox selection and single-click is that mouse interaction is routed through the `BehaviorService`, requiring interception at the behavior layer.

As a result, the typical strategies developers rely on are ineffective. These include:

- Overriding `WndProc`
- Overriding or handling mouse events (`OnMouseDown`, `OnClick`, etc.), even in the custom designer itself
- Using `IMessageFilter` and `PreFilterMessage`
- Overriding `CanAddComponent` or `CanBeParentedTo`
- Intercepting `OnDragDrop` (which only applies to drag-and-drop)

One approach that seems to work is to push a custom `Behavior` from within the designer's `Initialize` method:


```
public override void Initialize(IComponent component)
{
    base.Initialize(component);
    if (GetService(typeof(BehaviorService)) is BehaviorService behaviorSvc &&
        Control is not null)
    {
        behaviorSvc.PushBehavior(new ImmutableSurfaceBehavior(behaviorSvc, Control));
        Control?.Log($"BehaviorService is online.");
    }
    else
    {
        Control?.Log($"BehaviorService could not be obtained.");
    }
}
```
This `Behavior` implementation uses adorner-layer hit-testing to selectively suppress mouse interaction over the control. This condition is arbitrary, but in this case is inspecting the...

```
class ImmutableSurfaceBehavior : Behavior
{
    private readonly UserControlEx _control;
    private readonly BehaviorService? _behaviorService;

    public ImmutableSurfaceBehavior(BehaviorService behaviorSvc, UserControlEx control)
    {
        _control = control;
        _behaviorService = control.Site?.GetService(typeof(BehaviorService)) as BehaviorService;
    }

    public override bool OnMouseDown(Glyph? glyph, MouseButtons button, Point adornerCoordinates)
    {
        if (_behaviorService is not null)
        {
            var screenPoint = _behaviorService.AdornerWindowToScreen(adornerCoordinates);
            var clientPoint = _control.PointToClient(screenPoint);
            var bounds = new Rectangle(Point.Empty, _control.Size);

            if (_control?.ContentType == ContentType.Immutable && bounds.Contains(clientPoint))
            {
                if (_control.Site?.GetService(typeof(ISelectionService)) is ISelectionService selSvc)
                {
                    _control.BeginInvoke(() =>
                    {
                        selSvc.SetSelectedComponents(new[] { _control }, SelectionTypes.Primary);
                    });
                }
                return true;
            }
        }
        return base.OnMouseDown(glyph, button, adornerCoordinates);
    }
}
```

One approach that seems to work is pushing a custom Behavior from within the designer’s Initialize method, in order to swallow the MouseDown event and prevent toolbox-initiated control creation at the source.

___
_I'm posting my "best solution so far," but having said that, I'm always looking for a canonical "best approach" if there is one. My aim here is to release this control (that is, the "real" control this MRE is based on) in production — so the other request is: if you do happen to clone my [repo](https://github.com/IVSoftware/conditionally-designable-user-control), and something doesn't work, I'd really appreciate it if you'd report the issue there. That is, if the whole thing isn't too arcane to begin with..._  
___

What makes it tricky to suppress control addition via toolbox selection and single-click is that mouse interaction is routed through the `BehaviorService`, requiring interception at the behavior layer.

As a result, the typical strategies we've come to rely on are ineffective. These include:
- Overriding or handling mouse events (`OnMouseDown`, `OnClick`, etc.), even in the custom designer itself
- Using `IMessageFilter` and `PreFilterMessage`
- Overriding `WndProc`
- Overriding `CanAddComponent` or `CanBeParentedTo` in the custom designer
- Intercepting `OnDragDrop` (which only applies to drag-and-drop)

However, working around this can be fairly straightforward.
___

##### Push the Behavior

```csharp
public override void Initialize(IComponent component)
{
    base.Initialize(component);
    if (GetService(typeof(BehaviorService)) is BehaviorService behaviorSvc &&
        Control is not null)
    {
        behaviorSvc.PushBehavior(new ImmutableSurfaceBehavior(behaviorSvc, Control));
    }
}
```

##### Implement the Behavior

This `Behavior` implementation uses adorner-layer hit-testing to selectively suppress mouse interaction over the control. The condition shown here is based on a user-defined property `ContentType` on a subclassed control (`UserControlEx`), which determines whether the surface should be interactive at design time:

```csharp
class ImmutableSurfaceBehavior : Behavior
{
    private readonly UserControlEx _control;
    private readonly BehaviorService? _behaviorService;

    public ImmutableSurfaceBehavior(BehaviorService behaviorSvc, UserControlEx control)
    {
        _control = control;
        _behaviorService = control.Site?.GetService(typeof(BehaviorService)) as BehaviorService;
    }

    public override bool OnMouseDown(Glyph? glyph, MouseButtons button, Point adornerCoordinates)
    {
        if (_behaviorService is not null)
        {
            var screenPoint = _behaviorService.AdornerWindowToScreen(adornerCoordinates);
            var clientPoint = _control.PointToClient(screenPoint);
            var bounds = new Rectangle(Point.Empty, _control.Size);

            if (_control.ContentType == ContentType.Immutable && bounds.Contains(clientPoint))
            {
                if (_control.Site?.GetService(typeof(ISelectionService)) is ISelectionService selSvc)
                {
                    _control.BeginInvoke(() =>
                    {
                        selSvc.SetSelectedComponents(new[] { _control }, SelectionTypes.Primary);
                    });
                }
                return true;
            }
        }
        return base.OnMouseDown(glyph, button, adornerCoordinates);
    }
}
```
