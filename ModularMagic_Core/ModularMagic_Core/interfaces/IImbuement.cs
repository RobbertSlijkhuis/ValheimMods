namespace ModularMagic_Core.Interfaces
{
    internal interface IImbuement
    {
        bool GetEnabled();
        string GetName();
        //ImbuementPath GetPath();
        int GetSkillRequired();
        string GetType();
        string GetValue();
    }
}
