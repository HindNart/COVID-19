public interface IUpgradable
{
    void Upgrade();
    bool CanUpgrade(float resources);
}