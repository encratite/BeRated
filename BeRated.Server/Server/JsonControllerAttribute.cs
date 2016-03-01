namespace BeRated.Server
{
    class JsonControllerAttribute : ControllerAttribute
    {
        public JsonControllerAttribute()
            : base(RenderMethod.JsonSerialization)
        {
        }
    }
}
