//-----------------------------------------------------------------------
// <copyright file="IValidationHook.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinValidator.Editor
{
    using System;

    public interface IValidationHook
    {
        string Name { get; }
        void Hook(Action runner);
        void Unhook(Action runner);
        void StopTriggeringEvent();
    }
}
