namespace GlogGenerator.Data
{
    public interface IIgdbEntityReference
    {
        public bool HasIgdbEntityData();

        public string GetIgdbEntityDataId();

        public string GetReferenceableKey();
    }
}
