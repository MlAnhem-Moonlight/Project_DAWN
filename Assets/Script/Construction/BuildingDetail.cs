
using System;

[Serializable]
public class ResourceDataField
{
    public int wood;
    public int stone;
    public int iron;
    public int gold;
    public int meat;
}

[Serializable]
public class UpgradeDataField
{
    public ResourceDataField T1_T2;
    public ResourceDataField T2_T3;
}

[Serializable]
public class BuildingDataField
{
    public ResourceDataField T1_Total;
    public UpgradeDataField Upgrade;
}

[Serializable]
public class BuildingResourceFileField
{
    public BuildingDataField Fortress;
    public BuildingDataField Watchtown;
    public BuildingDataField Barrack;
    public BuildingDataField VillagerHouse;
    public BuildingDataField Tavern;
}