namespace NetworkServices
{
    public struct Const<T>
    {
        public T Value { get; }

        public Const(T value) : this()
        {
            this.Value = value;
        }
    }
}
