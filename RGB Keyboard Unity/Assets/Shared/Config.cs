using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CodeChicken.RGBKeyboard
{
	public static class Config
	{
		public static string path;

		static Config() {
			JsonConvert.DefaultSettings = () => new JsonSerializerSettings {
				Converters = new List<JsonConverter> {
					new VectorJsonConverter(),
					new KeyFrameJsonConverter(),
					new AnimationCurveJsonConverter(),
					new ColorJsonConverter()
				}
			};
		}

		public class VectorJsonConverter : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
				var _formatting = writer.Formatting;
				writer.Formatting = Formatting.None;
				writer.WriteStartArray();
				if (value is Vector2) {
					var v = (Vector2)value;
					writer.WriteValue(v.x);
					writer.WriteValue(v.y);
				}
#if UNITY_5
				else if(value is Vector3) {
					var v = (Vector3)value;
					writer.WriteValue(v.x);
					writer.WriteValue(v.y);
					writer.WriteValue(v.z);
				}
				else if (value is Vector4) {
					var v = (Vector4)value;
					writer.WriteValue(v.x);
					writer.WriteValue(v.y);
					writer.WriteValue(v.z);
					writer.WriteValue(v.w);
				}
#endif
				writer.WriteEndArray();
				writer.Formatting = _formatting;
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
				var arr = JArray.Load(reader);

				switch (arr.Count) {
					case 2:
						return new Vector2(arr[0].Value<float>(), arr[1].Value<float>());
#if UNITY_5
					case 3:
						return new Vector3(arr[0].Value<float>(), arr[1].Value<float>(), arr[2].Value<float>());
					case 4:
						return new Vector4(arr[0].Value<float>(), arr[1].Value<float>(), arr[2].Value<float>(), arr[3].Value<float>());
#endif
				}

				return null;
			}

			public override bool CanConvert(Type t) {
				return t == typeof(Vector2)
#if UNITY_5
					|| t == typeof(Vector3) || t == typeof(Vector4)
#endif
					;
			}
		}

		public class KeyFrameJsonConverter : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
				var obj = (Keyframe)value;
				new JObject {
					{"time", obj.time},
					{"value", obj.value},
					{"m0", obj.inTangent},
					{"m1", obj.outTangent}
				}.WriteTo(writer);
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
				var obj = JObject.Load(reader);
				return new Keyframe(
					obj["time"].Value<float>(),
					obj["value"].Value<float>(),
					obj["m0"].Value<float>(),
					obj["m1"].Value<float>());
			}

			public override bool CanConvert(Type t) {
				return t == typeof(Keyframe);
			}
		}

		public class AnimationCurveJsonConverter : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
				var obj = (AnimationCurve)value;
				serializer.Serialize(writer, obj.keys);
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
				return new AnimationCurve(serializer.Deserialize<Keyframe[]>(reader));
			}

			public override bool CanConvert(Type t) {
				return t == typeof(AnimationCurve);
			}
		}

		public class ColorJsonConverter : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
				var obj = value is Color ? (Color32) (Color) value : (Color32) value;
				writer.WriteValue(string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", obj.r, obj.g, obj.b, obj.a));
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
				var s = JToken.Load(reader).Value<string>();
				if (s[0] != '#') throw new Exception("Color must start with a #");
				if (s.Length != 9) throw new Exception("Color must be full #RGBA");
				uint rgba = uint.Parse(s.Substring(1), NumberStyles.HexNumber);
				var c = new Color32((byte)(rgba >> 24), (byte)(rgba >> 16), (byte)(rgba >> 8), (byte)(rgba));
				if (objectType == typeof(Color))
					return (Color)c;

				return c;
			}

			public override bool CanConvert(Type t) {
				return t == typeof(Color) || t == typeof(Color32);
			}
		}
	}
}
