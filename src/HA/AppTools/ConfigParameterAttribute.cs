namespace HA.AppTools;

[AttributeUsage(AttributeTargets.Property)]
public class ConfigParameterAttribute : Attribute
{
    private string _section;
    private string _name;
    private bool _required = false;

    public ConfigParameterAttribute(string? section, string? name)
    {
        _section = section ?? "";
        _name = name ?? "";
    }

    public virtual string Section
    {
        get { return _section; }
    }

    public virtual string Name
    {
        get { return _name; }
    }

    public virtual bool Required
    {
        get { return _required; }
        set { _required = value; }
    }

}