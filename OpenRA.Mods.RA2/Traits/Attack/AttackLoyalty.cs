#region Copyright & License Information
/*
 * Copyright 2007-2016 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Activities;
using OpenRA.Traits;
using OpenRA.Mods.Common.Traits;

namespace OpenRA.Mods.RA2.Traits
{
	[Desc("Can change owners of victims with `MindControllable` trait.")]
	public class AttackLoyaltyInfo : AttackBaseInfo
	{
		public readonly int TargetsAtOnce = 1;

		public override object Create(ActorInitializer init) { return new AttackLoyalty(init.Self, this); }
	}

	public class AttackLoyalty : AttackBase, INotifyKilled
	{
		readonly AttackLoyaltyInfo info;

		public AttackLoyalty(Actor self, AttackLoyaltyInfo info)
			: base(self, info)
		{
			this.info = info;
		}

		protected override bool CanAttack(Actor self, Target target)
		{
			if (target.Actor.TraitOrDefault<MindControllable>() == null)
				return false;

			return base.CanAttack(self, target);
		}

		public override Activity GetAttackActivity(Actor self, Target newTarget, bool allowMove, bool forceAttack)
		{
			return new MindControl(this, newTarget, info);
		}

		public List<Actor> Victims = new List<Actor>();

		class MindControl : Activity
		{
			readonly Target target;
			readonly AttackLoyalty attack;
			readonly AttackLoyaltyInfo info;

			public MindControl(AttackLoyalty attack, Target target, AttackLoyaltyInfo info)
			{
				this.target = target;
				this.attack = attack;
				this.info = info;
			}

			public override Activity Tick(Actor self)
			{
				if (IsCanceled || !target.IsValidFor(self) || !attack.CanAttack(self, target) || attack.Victims.Count > info.TargetsAtOnce)
					return NextActivity;

				var mindControllable = target.Actor.TraitOrDefault<MindControllable>();
				if (mindControllable != null)
				{
					attack.DoAttack(self, target);
					mindControllable.ChangeOwner(target.Actor, self);
					attack.Victims.Add(target.Actor);
				}

				return this;
			}
		}

		public void Killed(Actor self, AttackInfo e)
		{
			foreach (var victim in Victims)
			{
					var mindControllable = victim.TraitOrDefault<MindControllable>();
					if (mindControllable != null)
						mindControllable.RevertOwner(victim);
			}
		}
	}
}
