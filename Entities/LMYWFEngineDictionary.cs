namespace LMY.Workflow
{
    public class LMYWFEngineKeyValuePair
    {
        public LMYWFEngineKeyValuePair(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public object Value { get; set; }
    }

    public class LMYWFEngineDictionary
    {
        public LMYWFEngineKeyValuePair[] Pairs { get; set; }
        public string[] GetKeys(string prefix = "")
        {
            string[] names = new string[Pairs.Length];
            for (int i = 0; i < Pairs.Length; i++)
            {
                names[i] = prefix + Pairs[i].Key;
            }
            return names;
        }

        public string[] GetValues(string prefix = "")
        {
            string[] names = new string[Pairs.Length];
            for (int i = 0; i < Pairs.Length; i++)
            {
                names[i] = prefix + Pairs[i].Value;
            }
            return names;
        }
    }



}