namespace AssemblyToProcess
{
    public class ClassWithPropertiesDescendant : ClassWithProperties
    {
        // hides old number
        public double Number { get; set; }
        public string NewProperty { get; set; }
    }
}