using System.Linq;

namespace ET
{
    public class UnitComponentAwakeSystem: AwakeSystem<UnitComponent>
    {
        public override void Awake(UnitComponent self)
        {
        }
    }
    
    public class UnitComponentDestroySystem: DestroySystem<UnitComponent>
    {
        public override void Destroy(UnitComponent self)
        {
            self.idUnits.Clear();
        }
    }
    
    public static class UnitComponentSystem
    {
        public static void Add(this UnitComponent self, Unit unit)
        {
            unit.Parent = self;
            self.idUnits.Add(unit.Id, unit);
        }

        public static Unit Get(this UnitComponent self, long id)
        {
            self.idUnits.TryGetValue(id, out Unit unit);
            return unit;
        }

        public static void Remove(this UnitComponent self, long id)
        {
            Unit unit;
            self.idUnits.TryGetValue(id, out unit);
            self.idUnits.Remove(id);
            unit?.Dispose();
        }

        public static void RemoveNoDispose(this UnitComponent self, long id)
        {
            self.idUnits.Remove(id);
        }

        public static Unit[] GetAll(this UnitComponent self)
        {
            return self.idUnits.Values.ToArray();
        }
    }
}