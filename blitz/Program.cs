using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace emicoviBlitz
{
    class Program
    {
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            Bootstrap.Init(null);
        }


        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Active R;


        private static string[] JungleMobsList =
           {
            "SRU_Red", "SRU_Blue", "SRU_Dragon", "SRU_Baron", "SRU_Gromp",
            "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Krug", "Sru_Crab"
            };

        public static Menu Menu, SettingsMenu;

        public static AIHeroClient _Player { get { return ObjectManager.Player; } }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (_Player.ChampionName != "Blitzcrank")
                return;
            Chat.Print("emicovi's Blitzcrank - LOADED", Color.GreenYellow);


            Q = new Spell.Skillshot(SpellSlot.Q, 925, SkillShotType.Linear, 0, 1750);
            W = new Spell.Active(SpellSlot.W, 200);
            E = new Spell.Active(SpellSlot.E, 125);
            R = new Spell.Active(SpellSlot.R, 600);



            Menu = MainMenu.AddMenu("emicovi - Blitzcrank", "emicovi");
            Menu.AddGroupLabel("emicovi- Blitzcrank");
            Menu.AddSeparator();
            Menu.AddLabel("Developed By emicovi");
            SettingsMenu = Menu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddLabel("MISC");
            SettingsMenu.Add("FleeW", new CheckBox("FleeW", true));
            SettingsMenu.Add("AutoE", new CheckBox("AutoE", true));
            SettingsMenu.Add("showgrab", new CheckBox("Show statistcis", true));

            SettingsMenu.Add("ts", new CheckBox("Use Common Target Selector", true));
            SettingsMenu.AddLabel("Custom Target Selector");
            foreach (var unit in HeroManager.Enemies)
            {
                SettingsMenu.Add(unit.ChampionName, new CheckBox("Grab " + unit.ChampionName));
            }

            SettingsMenu.AddLabel("MISC");
            SettingsMenu.Add("qCC", new CheckBox("Auto Q cc & dash enemy", true));
            SettingsMenu.Add("minGrab", new Slider("Min range grab ({0}%)", 125, 250));
            SettingsMenu.Add("maxGrab", new Slider("Max range grab ({0}%)", 125, 925));

            SettingsMenu.Add("rCount", new CheckBox("Auto R if enemies in range", true));
            SettingsMenu.Add("enemis in range", new Slider("Min", 3, 0, 5));
            SettingsMenu.Add("afterGrab", new CheckBox("Auto R after grab", true));
          //  SettingsMenu.Add("afterAA", new CheckBox("Auto R before AA", true));
            SettingsMenu.Add("rks", new CheckBox("R ks", false));
            //SettingsMenu.Add("inter", new CheckBox("R OnPossibleToInterrupt", true));
            //SettingsMenu.Add("Gap", new CheckBox("R OnEnemyGapcloser", true));
            SettingsMenu.Add("Qdrawn", new CheckBox("Drawn Q"));
            SettingsMenu.Add("Edrawn", new CheckBox("Drawn E"));
            SettingsMenu.Add("Rdrawn", new CheckBox("Drawn R"));
            SettingsMenu.AddLabel("HARASS");
            SettingsMenu.Add("Qh", new CheckBox("Use Q", true));
            SettingsMenu.Add("Eh", new CheckBox("Use E", true));
            // SettingsMenu.AddLabel("LANE CLEAR");
            //SettingsMenu.Add("Rlc",new CheckBox("Lane Clear with R",true));
            //SettingsMenu.Add("minionw", new Slider("Min Minions to use R to Lane Clear",0, 1, 50));
            //SettingsMenu.Add("mana.lane", new Slider("Min Mana to use R to Lane Clear ({0}%)", 10, 100));
           SettingsMenu.AddLabel("SMART KS");
            SettingsMenu.Add("KSQ", new CheckBox("Use Q", false));
            SettingsMenu.Add("KSE", new CheckBox("Use E", false));
            SettingsMenu.Add("KSR", new CheckBox("Use R", false));
            SettingsMenu.AddLabel("AUTO SPELL");
            SettingsMenu.Add("auto", new CheckBox("Use AutoSpell", false));

            SettingsMenu.AddLabel("JUNGLE STEAL");
            SettingsMenu.Add("ksjg", new KeyBind("Q Steal Drag/Baron", false, KeyBind.BindTypes.HoldActive));
            if (Game.MapId == GameMapId.SummonersRift)
            {
                SettingsMenu.Add("SRU_Baron", new CheckBox("Baron"));
                SettingsMenu.Add("SRU_Dragon", new CheckBox("Dragon"));

            }





            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void KillSteal()
        {
            var targetq = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var useQ = SettingsMenu["KSQ"].Cast<CheckBox>().CurrentValue;

            if (useQ && Q.IsReady() && targetq.IsValidTarget(Q.Range) && !targetq.IsZombie && !targetq.IsInvulnerable &&
                targetq.Health <= _Player.GetSpellDamage(targetq, SpellSlot.Q))
            {
                Q.Cast(targetq);
            }
            var targetE = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            var useE = SettingsMenu["KSE"].Cast<CheckBox>().CurrentValue;

            if (useE && E.IsReady() && targetE.IsValidTarget(E.Range) && !targetE.IsZombie && !targetE.IsInvulnerable &&
                targetE.Health <= _Player.GetSpellDamage(targetE, SpellSlot.E))
            {
                E.Cast(targetE);
            }
            var targetR = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            var useR = SettingsMenu["KSR"].Cast<CheckBox>().CurrentValue;

            if (useR && R.IsReady() && targetR.IsValidTarget(R.Range) && !targetR.IsZombie && !targetR.IsInvulnerable &&
                targetR.Health <= _Player.GetSpellDamage(targetR, SpellSlot.R))

            {
                R.Cast(targetR);
            }
        }

        private static void Autospells()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null)
                return;
            if (target.IsInvulnerable)
                return;
            if (Q.IsReady() && target.IsValidTarget(Q.Range) && Q.GetPrediction(target).HitChancePercent >= 95 && target.IsValidTarget() && !target.HasBuffOfType(BuffType.SpellImmunity) &&
                    !target.HasBuffOfType(BuffType.SpellShield) &&)
            {
                Q.Cast(target);
                E.Cast(target);
            }
        }


        private static void Game_OnTick(EventArgs args)
        {
            if (_Player.IsDead || _Player.IsRecalling()) return;


            KillSteal();
            if (SettingsMenu["auto"].Cast<CheckBox>().CurrentValue) { Autospells(); }
            JgSteal();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                ComboQ();
                LogicR();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
               Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
               
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {

            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var Qdraw = SettingsMenu["Qdrawn"].Cast<CheckBox>().CurrentValue;
            var Edraw = SettingsMenu["Edrawn"].Cast<CheckBox>().CurrentValue;
            var Rdraw = SettingsMenu["Rdrawn"].Cast<CheckBox>().CurrentValue;


            if (Qdraw)
            {
                new Circle() {Color = Color.DeepSkyBlue, BorderWidth = 2, Radius = Q.Range}.Draw(_Player.Position);
            }
            if (Edraw)
            {
                new Circle() {Color = Color.DeepSkyBlue, BorderWidth = 2, Radius = E.Range}.Draw(_Player.Position);
            }
            if (Rdraw)
            {
                new Circle() {Color = Color.Red, BorderWidth = 2, Radius = R.Range}.Draw(_Player.Position);
            }
        }

        private static void JgSteal()
        {
            var useQ = SettingsMenu["ksjg"].Cast<KeyBind>().CurrentValue;
            if (Game.MapId == GameMapId.SummonersRift)
            {
                var t =
                    EntityManager.MinionsAndMonsters.Monsters.FirstOrDefault(
                        u => Q.IsInRange(u) && u.IsVisible && JungleMobsList.Contains(u.BaseSkinName));
                if (useQ)
                {
                    if (t.IsValidTarget()
                        && Q.IsReady()
                        && Q.IsInRange(t)
                        && t.Health <= _Player.GetSpellDamage(t, SpellSlot.Q))
                    {
                        Q.Cast(t);
                    }
                }
            }
        }



        /*  private static void afterAttack()
          {
              var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);

              if (SettingsMenu["afterAA"].Cast<CheckBox>().CurrentValue && R.IsReady() && !target.IsDead && !target.HasBuffOfType(BuffType.SpellShield) && !target.IsInvulnerable && target.IsValidTarget(R.Range))
              {
                  R.Cast(target);
              }
          }*/

        /* private static void Interrupter2_OnInterruptableTarget()
         {
             var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);
             if (R.IsReady() && SettingsMenu["inter"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(R.Range))
                 R.Cast(target);
         }*/



        /*private static void AntiGapcloser_OnEnemyGapcloser(G)
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            if (R.IsReady() && SettingsMenu["Gap"].Cast<CheckBox>().CurrentValue && 
                R.Cast(target);
        }*/



        /* private void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
         {
             if (E.IsReady() && args.Target.IsValid<Obj_AI_Hero>() && Config.Item("autoE", true).GetValue<bool>())
                 E.Cast();
         }*/

        private static void ComboQ()
        {
            var maxGrab = SettingsMenu["maxGrab"].Cast<Slider>().CurrentValue;
            var minGrab = SettingsMenu["minGrab"].Cast<Slider>().CurrentValue;

            if (SettingsMenu["ts"].Cast<CheckBox>().CurrentValue)
            {
                var t = TargetSelector.GetTarget(maxGrab, DamageType.Magical);

                if (t.IsValidTarget() && !t.HasBuffOfType(BuffType.SpellImmunity) &&
                    !t.HasBuffOfType(BuffType.SpellShield) && Q.IsReady() && Q.GetPrediction(t).HitChancePercent >= 90)
                {
                    Q.Cast(Q.GetPrediction(t).CastPosition);

                }
                if (SettingsMenu["AutoE"].Cast<CheckBox>().CurrentValue && E.IsReady() && t.IsValidTarget(E.Range))
                {
                    E.Cast();

                }
                if (SettingsMenu["afterGrab"].Cast<CheckBox>().CurrentValue && R.IsReady() && t.IsValidTarget(R.Range))
                {
                    R.Cast();

                }
            }

            foreach (var t in HeroManager.Enemies.Where(t => t.IsValidTarget(maxGrab)))
            {
                if (!t.HasBuffOfType(BuffType.SpellImmunity) && !t.HasBuffOfType(BuffType.SpellShield) && !SettingsMenu["ts"].Cast<CheckBox>().CurrentValue)
                {
                    if (SettingsMenu[t.ChampionName].Cast<CheckBox>().CurrentValue)
                    {
                        if (t.IsValidTarget() && Q.IsReady())
                        {
                            Q.Cast(t);
                        }
                        if (SettingsMenu["AutoE"].Cast<CheckBox>().CurrentValue && E.IsReady() &&
                            t.IsValidTarget(E.Range))
                        {
                            E.Cast();

                        }
                        if (SettingsMenu["afterGrab"].Cast<CheckBox>().CurrentValue && R.IsReady() &&
                            t.IsValidTarget(R.Range))
                        {
                            R.Cast();
                        }
                    }

                    if (SettingsMenu["qCC"].Cast<CheckBox>().CurrentValue)
                    {
                        if (!t.CanMove)
                        {
                            Q.Cast(t);
                            if (SettingsMenu["AutoE"].Cast<CheckBox>().CurrentValue && E.IsReady() && t.IsValidTarget(E.Range))
                            {
                                E.Cast();

                            }
                            if (SettingsMenu["afterGrab"].Cast<CheckBox>().CurrentValue && R.IsReady() && t.IsValidTarget(R.Range))
                            {
                                R.Cast();
                            }

                        }
                    }
                }

            }
        }


        private static void LogicR()
        {
            bool rKs = SettingsMenu["rKs"].Cast<CheckBox>().CurrentValue;
            foreach (var target in HeroManager.Enemies.Where(target => target.IsValidTarget(R.Range)))
            {
                if (rKs && !target.IsZombie && !target.IsInvulnerable &&
                target.Health <= _Player.GetSpellDamage(target, SpellSlot.R) &&target.IsValidTarget(R.Range))
                    R.Cast();
            }
            if (_Player.CountEnemiesInRange(R.Range) >= SettingsMenu["rCount"].Cast<Slider>().CurrentValue && SettingsMenu["rCount"].Cast<Slider>().CurrentValue > 0)
                R.Cast();
        }



        private static void Flee()
        {
            var useW = SettingsMenu["FleeW"].Cast<CheckBox>().CurrentValue;
            if (useW && W.IsReady())
            {
                W.Cast();
            }
        }

        private static void Harass()
        {
            var useQ = SettingsMenu["Qh"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Eh"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range) && Q.GetPrediction(target).HitChance >= HitChance.High)
            {
                Q.Cast(target);
                if (target.HasBuffOfType(BuffType.Snare)
                || target.HasBuffOfType(BuffType.Suppression)
                || target.HasBuffOfType(BuffType.Taunt)
                || target.HasBuffOfType(BuffType.Stun)
                || target.HasBuffOfType(BuffType.Charm)
                || target.HasBuffOfType(BuffType.Fear))

                    E.Cast(target);
            }
            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsZombie && !target.IsInvulnerable &&
                    !target.IsDead)
            {
                E.Cast(target);
            }
        }




    }

}