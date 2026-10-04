using UnityEngine;

public class ShowIfAttribute : PropertyAttribute
{
    public enum Operator { Equals, NotEquals, Greater, Less, GreaterOrEqual, LessOrEqual }
    public string FieldName;
    public Operator Op;
    public object ExpectedValue;
    public ShowIfAttribute(string fieldName)
    {
        FieldName = fieldName;
        ExpectedValue = true; 
        Op = Operator.Equals;
    }

    public ShowIfAttribute(string fieldName, object expectedValue, Operator op = Operator.Equals)
    {
        FieldName = fieldName;
        ExpectedValue = expectedValue;
        Op = op;
    }
}