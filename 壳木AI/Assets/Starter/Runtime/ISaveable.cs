namespace Starter.Runtime
{
    // 实现此接口的组件可参与存档/读档流程
    // 推荐继承 SaveableBehaviour 而非直接实现此接口，可自动完成注册/注销
    public interface ISaveable
    {
        // 唯一标识，用于在 SaveData 中区分不同组件的数据
        // 示例："Player"、"QuestSystem"、"Inventory_slot_0"
        string SaveID { get; }

        // SaveManager.Save() 时被调用，将本组件数据写入 data
        void OnSave(SaveData data);

        // SaveManager.Load() 时被调用，从 data 读取本组件数据
        void OnLoad(SaveData data);
    }
}
