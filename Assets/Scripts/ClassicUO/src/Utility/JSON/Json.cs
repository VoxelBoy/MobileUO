namespace TinyJson
{
	public static class Json {
		public static T Decode<T>(this string json) {
			if (string.IsNullOrEmpty(json)) return default(T);
			object jsonObj = JsonParser.ParseValue(json);
			if (jsonObj == null) return default(T);
			T obj = JsonMapper.DecodeJsonObject<T>(jsonObj);
			TypeExtensions.typeToProperties.Clear();
			return obj;
		}
		
		public static string Encode(this object value, bool pretty = false) {
			JsonBuilder builder = new JsonBuilder(pretty);
			JsonMapper.EncodeValue(value, builder);
			return builder.ToString();
		}
	}
}

