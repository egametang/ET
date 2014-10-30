using System;
using System.Collections.Generic;
using Common.Event;
using Model;
using MongoDB.Bson;

namespace Controller
{
    /// <summary>
    /// 控制复杂的buff逻辑,可以reload
    /// </summary>
    public static class BuffComponentExtension
    {
        public static void Add(this BuffComponent buffComponent, Buff buff)
        {
            if (buffComponent.buffs.Contains(buff))
            {
                throw new ArgumentException(string.Format("already exist same buff, Id: {0} ConfigId: {1}", buff.Id, buff.Config.Id));
            }

            if (buffComponent.idBuff.ContainsKey(buff.Id))
            {
                throw new ArgumentException(string.Format("already exist same buff, Id: {0} ConfigId: {1}", buff.Id, buff.Config.Id));
            }

            Env env = new Env();
            env[EnvKey.Unit] = buffComponent.Owner;
            env[EnvKey.Buff] = buff;

            World.Instance.GetComponent<EventComponent<WorldEventAttribute>>().Trigger(WorldEventType.BeforeAddBuff, env);

            buffComponent.buffs.Add(buff);
            buffComponent.idBuff.Add(buff.Id, buff);
            buffComponent.typeBuff.Add(buff.Config.Type, buff);

            World.Instance.GetComponent<EventComponent<WorldEventAttribute>>().Trigger(WorldEventType.AfterAddBuff, env);
        }

        public static Buff GetById(this BuffComponent buffComponent, ObjectId id)
        {
            if (!buffComponent.idBuff.ContainsKey(id))
            {
                return null;
            }

            return buffComponent.idBuff[id];
        }

        public static Buff GetOneByType(this BuffComponent buffComponent, BuffType type)
        {
            return buffComponent.typeBuff.GetOne(type);
        }

        public static Buff[] GetByType(this BuffComponent buffComponent, BuffType type)
        {
            return buffComponent.typeBuff.GetByKey(type);
        }

        private static bool Remove(this BuffComponent buffComponent, Buff buff)
        {
            if (buff == null)
            {
                return false;
            }

            Env env = new Env();
            env[EnvKey.Unit] = buffComponent.Owner;
            env[EnvKey.Buff] = buff;

            World.Instance.GetComponent<EventComponent<UnitEventAttribute>>().Trigger(WorldEventType.BeforeRemoveBuff, env);

            buffComponent.buffs.Remove(buff);
            buffComponent.idBuff.Remove(buff.Id);
            buffComponent.typeBuff.Remove(buff.Config.Type, buff);

            World.Instance.GetComponent<EventComponent<UnitEventAttribute>>().Trigger(WorldEventType.AfterRemoveBuff, env);

            return true;
        }

        public static bool RemoveById(this BuffComponent buffComponent, ObjectId id)
        {
            Buff buff = buffComponent.GetById(id);
            return buffComponent.Remove(buff);
        }

        public static void RemoveByType(this BuffComponent buffComponent, BuffType type)
        {
            Buff[] allbuffs = buffComponent.GetByType(type);
            foreach (Buff buff in allbuffs)
            {
                buffComponent.Remove(buff);
            }
        }
    }
}