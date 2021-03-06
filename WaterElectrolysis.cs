﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using xiaoye97;
using UnityEngine;
using System.Reflection;
using BepInEx.Configuration;

namespace Water_electrolysis
{
    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Gnimaerd.DSP.plugin.WaterElectrolysis", "WaterElectrolysis", "1.2")]
    public class WaterElectrolysis : BaseUnityPlugin
    {
        private Sprite icon;
        private static ConfigEntry<bool> BalanceAdjustment;
        void Start()
        {
            var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Water_electrolysis.waterelecicon"));
            WaterElectrolysis.BalanceAdjustment = Config.Bind<bool>("config", "BalanceAdjustment", true, "是否（true或false）进行氢气能量和轨道采集器功耗等平衡调整。Whether to change the hydrogen energy and the power consumption of the orbital collector (by setting true or false.)");
            icon = ab.LoadAsset<Sprite>("WaterElec3");
            if(WaterElectrolysis.BalanceAdjustment.Value)
            {
                LDBTool.EditDataAction += ChangeHeat;
                LDBTool.EditDataAction += ChangeCollectorEnergyNeed;
            }
            LDBTool.PreAddDataAction += AddTranslate;
            LDBTool.PostAddDataAction += AddWaterToH;
        }

        void ChangeHeat(Proto proto)
        {
            if(!BalanceAdjustment.Value)
            {
                return;
            }
            if (proto is ItemProto && proto.ID == 1120)
            {
                var itemp = proto as ItemProto;
                itemp.HeatValue = 439600;
            }
            else if (proto is ItemProto && proto.ID == 1114)
            {
                var itemp = proto as ItemProto;
                itemp.HeatValue = 8000000;
            }
        }

        void ChangeCollectorEnergyNeed(Proto proto)
        {
            if (!BalanceAdjustment.Value)
            {
                return;
            }
            if (proto is ItemProto && proto.ID == 2105)
            {
                var clct = proto as ItemProto;
                clct.prefabDesc.workEnergyPerTick = 30000;
            }
        }

        void AddWaterToH()
        {
            var ori = LDB.recipes.Select(23);
            //var x_icon = LDB.recipes.Select(58);

            var waterele = ori.Copy();
            waterele.ID = 443;
            waterele.Explicit = true;
            waterele.Name = "催化电解";
            waterele.name = "催化电解".Translate();
            waterele.Items = new int[] { 1000 };
            waterele.ItemCounts = new int[] { 1 };
            waterele.Results = new int[] { 1120 };
            waterele.ResultCounts = new int[] { 1 };
            waterele.GridIndex = 1110;
            waterele.SID = "1110";
            waterele.sid = "1110".Translate();
            Traverse.Create(waterele).Field("_iconSprite").SetValue(icon);
            waterele.TimeSpend = 30;
            waterele.Description = "催化电解描述";
            waterele.description = "催化电解描述".Translate();
            waterele.preTech = LDB.techs.Select(1121);

            //氢气的合成公式里加入这个公式
            var h = LDB.items.Select(1120);
            h.recipes.Add(waterele);

            LDBTool.PostAddProto(ProtoType.Recipe, waterele);

        }

        void AddTranslate()
        {
            StringProto recipeName = new StringProto();
            StringProto desc = new StringProto();
            recipeName.ID = 28001;
            recipeName.Name = "催化电解";
            recipeName.name = "催化电解";
            recipeName.ZHCN = "催化电解";
            recipeName.ENUS = "Water Electrolysis";
            recipeName.FRFR = "Water Electrolysis";

            desc.ID = 28002;
            desc.Name = "催化电解描述";
            desc.name = "催化电解描述";
            desc.ZHCN = "电解水并获取氢气。";
            desc.ENUS = "Electrolysis of water to produce hydrogen.";
            desc.FRFR = "Electrolysis of water to produce hydrogen.";

            LDBTool.PreAddProto(ProtoType.String, recipeName);
            LDBTool.PreAddProto(ProtoType.String, desc);
        }
    }
}
