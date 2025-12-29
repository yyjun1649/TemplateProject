using Library;

public class TStatModifier<T> : PooledDisposable
{
    public string ModifierId { get; private set; }
    public eModifierType ModifierType{ get; private set; }
    public T StatType{ get; private set; }
    public float Value{ get; set; }
    public bool IsStackable { get; private set; }

    public void Set(string name, eModifierType modifierType, T statType, float value, bool isStackable = false)
    {
        ModifierId = name;
        ModifierType = modifierType;
        StatType = statType;
        Value = value;
        IsStackable = isStackable;
    }

    protected override void Reset()
    {
        ModifierId = string.Empty;
        ModifierType = eModifierType.Additive;
        Value = 0;
        IsStackable = false;
    }
    
    public static TStatModifier<T> MakeModifier(string id, eModifierType modifierType, T statType, float value, bool isStackable = false)
    {
        var modifier = Get<TStatModifier<T>>();
        
        modifier.Set(id,modifierType,statType,value);

        return modifier;
    }
}