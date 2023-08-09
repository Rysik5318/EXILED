// -----------------------------------------------------------------------
// <copyright file="StalkVigorUse.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Generic.Scp106API
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using API.Features.Pools;
    using Exiled.API.Features;
    using HarmonyLib;
    using PlayerRoles.PlayableScps.Scp106;
    using PlayerRoles.PlayableScps.Subroutines;

    using static HarmonyLib.AccessTools;

    using BaseScp106Role = PlayerRoles.PlayableScps.Scp106.Scp106Role;
    using Scp106Role = API.Features.Roles.Scp106Role;

    /// <summary>
    /// Patches <see cref="Scp106StalkAbility.UpdateServerside"/>.
    /// Adds the <see cref="Scp106Role.VigorStalkCostMoving" /> and <see cref="Scp106Role.VigorStalkCostStationary" /> property.
    /// </summary>
    [HarmonyPatch(typeof(Scp106StalkAbility), nameof(Scp106StalkAbility.UpdateServerside))]
    internal class StalkVigorUse
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            LocalBuilder scp049Role = generator.DeclareLocal(typeof(Scp106Role));

            // replace "float num = base.ScpRole.FpcModule.Motor.MovementDetected ? 0.09f : 0.06f;"
            // with
            // Scp106Role = Player.Get(this.Owner).Role.As<Scp106Role>()
            // "float num = base.ScpRole.FpcModule.Motor.MovementDetected ? Scp106Role.VigorStalkCostMoving : Scp106Role.VigorStalkCostStationary;"
            int offset = 0;
            int index = newInstructions.FindIndex(instruction => instruction.opcode == OpCodes.Ldc_R4) + offset;
            newInstructions.RemoveAt(index);

            newInstructions.InsertRange(
                index,
                new CodeInstruction[]
                {
                    // Player.Get(base.Owner).Role
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Call, PropertyGetter(typeof(ScpStandardSubroutine<BaseScp106Role>), nameof(ScpStandardSubroutine<BaseScp106Role>.Owner))),
                    new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(Player), nameof(Player.Role))),

                    // (Player.Get(base.Owner).Role as Scp106Role).VigorStalkCostMoving
                    new(OpCodes.Isinst, typeof(Scp106Role)),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(Scp106Role), nameof(Scp106Role.VigorStalkCostMoving))),
                });

            offset = 0;
            index = newInstructions.FindIndex(instruction => instruction.opcode == OpCodes.Ldc_R4) + offset;
            newInstructions.RemoveAt(index);

            newInstructions.InsertRange(
                index,
                new CodeInstruction[]
                {
                    // Player.Get(base.Owner).Role
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Call, PropertyGetter(typeof(ScpStandardSubroutine<BaseScp106Role>), nameof(ScpStandardSubroutine<BaseScp106Role>.Owner))),
                    new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(Player), nameof(Player.Role))),

                    // (Player.Get(base.Owner).Role as Scp106Role).VigorStalkCostStationary
                    new(OpCodes.Isinst, typeof(Scp106Role)),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(Scp106Role), nameof(Scp106Role.VigorStalkCostStationary))),
                });
            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}
