using Microsoft.DotNet.DesignTools.Designers.Behaviors;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConditionallyDesignable
{
    class ImmutableSurfaceBehavior : Behavior
    {
        private readonly UserControlEx _control;
        private readonly BehaviorService? _behaviorService;

        public ImmutableSurfaceBehavior(BehaviorService behaviorSvc, UserControlEx control)
        {
            _control = control;
            _control?.Log($"{nameof(ImmutableSurfaceBehavior)}: CTOR");
            _behaviorService = control.Site?.GetService(typeof(BehaviorService)) as BehaviorService;
        }

        public override bool OnMouseDown(Glyph? glyph, MouseButtons button, Point adornerCoordinates)
        {
            if (_behaviorService is not null)
            {
                var screenPoint = _behaviorService.AdornerWindowToScreen(adornerCoordinates);
                var clientPoint = _control.PointToClient(screenPoint);
                var bounds = new Rectangle(Point.Empty, _control.Size);

                _control?.Log($"{nameof(OnMouseDown)}: Button={button}");
                _control?.Log($"  AdornerCoords: {adornerCoordinates}");
                _control?.Log($"  ScreenPoint: {screenPoint}");
                _control?.Log($"  ClientPoint: {clientPoint}");
                _control?.Log($"  Control.Bounds: {bounds}");

                if (_control?.ContentType == ContentType.Immutable && bounds.Contains(clientPoint))
                {
                    if (_control.Site?.GetService(typeof(ISelectionService)) is ISelectionService selSvc)
                    {
                        _control.BeginInvoke(() =>
                        {
                            selSvc.SetSelectedComponents(new[] { _control }, SelectionTypes.Primary);
                            _control?.Log($"{nameof(OnMouseDown)}: Selected self");
                        });
                    }

                    _control?.Log($"{nameof(OnMouseDown)}: DISALLOWED");
                    return true;
                }
            }

            _control?.Log($"{nameof(OnMouseDown)}: ALLOWED");
            return base.OnMouseDown(glyph, button, adornerCoordinates);
        }
    }
}
