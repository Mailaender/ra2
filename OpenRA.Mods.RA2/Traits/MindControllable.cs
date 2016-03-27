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

using OpenRA.Traits;

namespace OpenRA.Mods.RA2.Traits
{
	[Desc("Interacts with the `AttackLoyality` trait.")]
	public class MindControllableInfo : ITraitInfo
	{
		public object Create(ActorInitializer init) { return new MindControllable(init.Self, this); }
	}

	public class MindControllable : INotifyOwnerChanged, INotifyKilled
	{
		Player originalOwner;
		Player changingOwner;

		Actor controller;

		public MindControllable(Actor self, MindControllableInfo info)
		{
			originalOwner = self.Owner;
		}

		public void ChangeOwner(Actor self, Actor attacker)
		{
			controller = attacker;
			changingOwner = attacker.Owner;
			self.ChangeOwner(attacker.Owner);
			self.CancelActivity(); // Stop shooting, you have got new enemies
		}

		public void RevertOwner(Actor self)
		{
			changingOwner = originalOwner;
			self.ChangeOwner(originalOwner);
		}

		public void OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			if (changingOwner == null || changingOwner != newOwner)
				originalOwner = newOwner; // It wasn't a temporary change, so we need to update here
			else
				changingOwner = null; // It was triggered by this trait: reset
		}

		public void Killed(Actor self, AttackInfo e)
		{
			var mindController = controller.TraitOrDefault<AttackLoyalty>();
			if (mindController != null)
				mindController.Victims.Remove(self);
		}
	}
}
