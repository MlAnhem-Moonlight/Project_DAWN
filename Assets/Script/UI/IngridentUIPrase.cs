using System;
using System.Collections.Generic;

[Serializable]
public class UnitCostLevel
{
    public int wood;
    public int stone;
    public int iron;
    public int gold;
}

[Serializable]
public class UnitCostItem
{
    public int MeatPerRound;
    public UnitCostLevel Lv1;
    public UnitCostLevel Lv2;
    public UnitCostLevel Lv3;
    public UnitCostLevel Lv4;
    public UnitCostLevel Lv5;
    public UnitCostLevel Lv6;
    public UnitCostLevel Lv7;
    public UnitCostLevel Lv8;
}

[Serializable]
public class UnitCostWrapper
{
    public Dictionary<string, UnitCostItem> Units;
}

[Serializable]
public class UnitCostWrapperRaw
{
    public List<UnitCostItemRaw> Units;
}

[Serializable]
public class UnitCostItemRaw
{
    public string name;
    public int MeatPerRound;
    public UnitCostLevel Lv1;
    public UnitCostLevel Lv2;
    public UnitCostLevel Lv3;
    public UnitCostLevel Lv4;
    public UnitCostLevel Lv5;
    public UnitCostLevel Lv6;
    public UnitCostLevel Lv7;
    public UnitCostLevel Lv8;
}
[Serializable]
public class UnitUpgradeLevel
{
    public int wood;
    public int stone;
    public int iron;
    public int gold;
}

[Serializable]
public class UpgradeKeyValue
{
    public string key;               // ví dụ: "Lv1_2"
    public int wood;
    public int stone;
    public int iron;
    public int gold;
}

[Serializable]
public class UnitUpgradeEntry
{
    public string name;
    public List<UpgradeKeyValue> upgrade;
}

[Serializable]
public class UnitUpgradeData
{
    public List<UnitUpgradeEntry> Units;
}
