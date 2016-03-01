using System;

namespace BeRated.Server
{
    class ControllerAttribute : Attribute
    {
        public RenderMethod RenderMethod { get; private set; }

        public ControllerAttribute(RenderMethod renderMethod = RenderMethod.RazorTemplate)
        {
            RenderMethod = renderMethod;
        }
    }
}
