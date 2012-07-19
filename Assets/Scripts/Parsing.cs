using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

public static class Parsing
{
    public static bool ParseVector3(string literal, ref Vector3 result)
    {
        //Debug.Log("Parsing " + literal + " as Decimal3");    	    	
        var i = 0;
        return ParseVector3(literal, ref i, ref result);
    }
    static bool ParseVector3(string literal, ref int from, ref Vector3 result)
    {
        if (!ParseDecimal(literal, ref from, ref result.x) ||
            !ParseDecimal(literal, ref from, ref result.y) ||
            !ParseDecimal(literal, ref from, ref result.z))
        {
            Debug.LogWarning("Parsing '" + literal + "' as Vector3 failed; skipping the rest.");
            return false;
        }
        return true;
    }

    public static bool ParseRgb(string literal, ref Color result)
    {
        //Debug.Log("Parsing " + literal + " as Rgb");    	    	
        var i = 0;
        return ParseRgb(literal, ref i, ref result);
    }
    static bool ParseRgb(string literal, ref int from, ref Color result)
    {
        if (!ParseByte(literal, ref from, ref result.r) ||
            !ParseByte(literal, ref from, ref result.g) ||
            !ParseByte(literal, ref from, ref result.b))
        {
            Debug.LogWarning("Parsing '" + literal + "' as Rgb failed; skipping the rest.");
            return false;
        }
        return true;
    }

    public static bool ParseRgba(string literal, ref Color result)
    {
        //Debug.Log("Parsing " + literal + " as Rgba");        	
        var i = 0;
        return ParseRgba(literal, ref i, ref result);
    }
    public static bool ParseRgba(string literal, ref int from, ref Color result)
    {
        if (!ParseRgb(literal, ref from, ref result) || !ParseByte(literal, ref from, ref result.a))
        {
            Debug.LogWarning("Parsing '" + literal + "' as Rgba failed; skipping the rest.");
            return false;
        }
        return true;
    }

    public static bool ParseFogParameters(string literal, ref FogParameters result)
    {
        //Debug.Log("Parsing " + literal + " as FogParams");
        var i = 0;
        if (!ParseRgb(literal, ref i, ref result.Color) || !ParseDecimal(literal, ref i, ref result.Density))
        {
            Debug.LogWarning("Parsing '" + literal + "' as FogParameters failed; skipping the rest.");
            return false;
        }
        return true;
    }
    
    public static IEnumerable<OzmlMaterial> ParseMaterials(string literal)
    {
    	List<OzmlMaterial> result = new List<OzmlMaterial>();
    	
    	var start = literal.IndexOf('.');
    	
    	while (start >= 0)
    	{
			var propStart = literal.IndexOf('{', start);
			var propEnd = literal.IndexOf('}', start);
			if (propEnd > propStart && propStart > start)
				result.Add(ParseMaterial(literal.Substring(start, propEnd - start + 1)));
			
			start = literal.IndexOf('.', propEnd);
    	}
    	
    	return result.ToArray();
    }
    static OzmlMaterial ParseMaterial(string literal) 
    {
        //Debug.Log("Parsing " + literal + " as Material");

    	var result = new OzmlMaterial();
    	
		var bracketStart = literal.IndexOf('{');
		result.Name = literal.Substring(1, bracketStart - 1).Trim();		
		
		var tag = "diffuse:";
		var from = literal.IndexOf(tag, bracketStart) + tag.Length;
        if (from >= tag.Length)
            ParseRgb(literal, ref from, ref result.AmbientDiffuse);
		
		tag = "specular:";
		from = literal.IndexOf(tag, bracketStart) + tag.Length;
        if (from >= tag.Length)
            ParseRgb(literal, ref from, ref result.Specular);
		
		tag = "emissive:";
		from = literal.IndexOf(tag, bracketStart) + tag.Length;
		if (from >= tag.Length)
            ParseRgb(literal, ref from, ref result.Emissive);
		
		tag = "opacity:";
		from = literal.IndexOf(tag, bracketStart) + tag.Length;
        if (from >= tag.Length)
            ParseByte(literal, ref from, ref result.Opacity);

        tag = "power:";
		from = literal.IndexOf(tag, bracketStart) + tag.Length;
        if (from >= tag.Length)
            ParseDecimal(literal, ref from, ref result.Power);
		
		tag = "texture:";
		from = literal.IndexOf(tag, bracketStart) + tag.Length;
        if (from >= tag.Length)
			result.Texture = literal.Substring( from, literal.IndexOf('}') - from ).Trim();	

        return result;
    }

    public static bool ParseDecimal(string literal, ref float result)
    {
        //Debug.Log("Parsing " + literal + " as Decimal");    	
        int i = 0;
        return ParseDecimal(literal, ref i, ref result);
    }
    static bool ParseDecimal(string literal, ref int from, ref float result)
    {
    	int to = -1;
        var toSep = literal.IndexOf(',', from); if (toSep >= 0 && (to == -1 || toSep < to)) to = toSep;
		toSep = literal.IndexOf(';', from); if (toSep >= 0 && (to == -1 || toSep < to)) to = toSep;
        toSep = literal.IndexOf('}', from); if (toSep >= 0 && (to == -1 || toSep < to)) to = toSep;                
        var substring = to < 0 ? literal.Substring(from) : literal.Substring(from, to - from);
        from = to + 1;
        float value;
        if (!float.TryParse(substring, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            Debug.LogWarning("Parsing a Decimal in '" + literal + "' failed.");
            return false;
        }
        result = value;
        return true;
    }

    public static bool ParseByte(string literal, ref float result)
    {
        //Debug.Log("Parsing " + literal + " as Byte");    	
        int i = 0;
        return ParseByte(literal, ref i, ref result);
    }
    static bool ParseByte(string literal, ref int from, ref float result)
    {
    	int to = -1;
        var toSep = literal.IndexOf(',', from); if (toSep >= 0 && (to == -1 || toSep < to)) to = toSep;
		toSep = literal.IndexOf(';', from); if (toSep >= 0 && (to == -1 || toSep < to)) to = toSep;
        toSep = literal.IndexOf('}', from); if (toSep >= 0 && (to == -1 || toSep < to)) to = toSep;
        var substring = to < 0 ? literal.Substring(from) : literal.Substring(from, to - from);
        from = to + 1;
        if (substring.IndexOf('.') >= 0)
        {
            float fValue;
            if (float.TryParse(substring, NumberStyles.Float, CultureInfo.InvariantCulture, out fValue) && fValue <= 1 && fValue >= 0)
            {
                result = fValue;
                return true;
            }
            Debug.LogWarning("Parsing a Byte as Decimal in '" + literal + "' failed.");
            return false;
        }
        byte bValue;
        if (byte.TryParse(substring, NumberStyles.Integer, CultureInfo.InvariantCulture, out bValue) && bValue <= 255 && bValue >= 0)
        {
            result = bValue / 255f;
            return true;
        }
        Debug.LogWarning("Parsing a Byte in '" + literal + "' failed.");
        return false;
    }   
}
