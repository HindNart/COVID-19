public interface IUpgradable
{
    void Upgrade();
    bool CanUpgrade(int resources);
}