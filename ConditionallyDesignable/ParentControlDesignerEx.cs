using Microsoft.DotNet.DesignTools.Designers.Behaviors;
using Microsoft.DotNet.DesignTools.Designers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConditionallyDesignable
{
    class ParentControlDesignerEx : ParentControlDesigner
    {
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            // Control should exist now.
            Control?.Log($"{DateTime.Now}", append: false);
            Control?.Log($"Version 1.2.3");

            if (GetService(typeof(BehaviorService)) is BehaviorService behaviorSvc &&
                Control is not null)
            {
                var behavior = new ImmutableSurfaceBehavior(behaviorSvc, Control);
                behaviorSvc.PushBehavior(behavior);
                Control?.Log($"BehaviorService is online.");
            }
            else
            {
                Control?.Log($"BehaviorService could not be obtained.");
            }
        }
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
    }
}
