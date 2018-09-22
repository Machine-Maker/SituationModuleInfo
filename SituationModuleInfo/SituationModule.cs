// ScienceSituationInfo.SituationModule
using ScienceSituationInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.EditorAny, false)]
public class SituationModule : MonoBehaviour
{
    private readonly string _biomeDepending = " Biome Dpnd";

    private string _usageMaskInt = "";

    private List<AvailablePart> _partsWithScience = new List<AvailablePart>();

    private List<AvailablePart> _partsWithScienceSEP = new List<AvailablePart>();

    private List<AvailablePart> _partsWithScienceDM = new List<AvailablePart>();

    private List<AvailablePart> _partsWithScienceIMP = new List<AvailablePart>();

    private List<AvailablePart> _partsWithScienceROV = new List<AvailablePart>();

    protected static bool SEP_Present;

    protected static bool DMagic_Present;

    protected static bool DMagic_Present_X;

    protected static bool Impact_Present;

    protected static bool Impact_Present_X;

    protected static bool ROV_Present;

    protected static bool ROV_Present_X;

    private void Start()
    {
        SEP_Present = AssemblyLoader.loadedAssemblies.Any((AssemblyLoader.LoadedAssembly a) => a.assembly.GetName().Name == "SEPScience");
        DMagic_Present = AssemblyLoader.loadedAssemblies.Any((AssemblyLoader.LoadedAssembly a) => a.assembly.GetName().Name == "DMagic");
        Impact_Present = AssemblyLoader.loadedAssemblies.Any((AssemblyLoader.LoadedAssembly a) => a.assembly.GetName().Name == "kerbal-impact");
        ROV_Present = AssemblyLoader.loadedAssemblies.Any((AssemblyLoader.LoadedAssembly a) => a.assembly.GetName().Name == "RoverScience");
        RoverScience();
        Impact();
        List<AvailablePart>.Enumerator enumerator;
        if (DMagic_Present && !DMagic_Present_X)
        {
            _partsWithScienceDM = (from p in PartLoader.LoadedPartsList
                                   where p.name.Contains("dmSeismic")
                                   select p).ToList();
            enumerator = _partsWithScienceDM.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    AvailablePart current = enumerator.Current;
                    ScienceExperiment experiment = new ScienceExperiment();
                    string name = current.name;
                    if (!(name == "dmSeismicPod"))
                    {
                        if (name == "dmSeismicHammer")
                        {
                            experiment = ResearchAndDevelopment.GetExperiment("dmseismicHammer");
                            _usageMaskInt = "<b><color=red>SURFACE ONLY, handled by EVA</color></b>";
                        }
                    }
                    else
                    {
                        experiment = ResearchAndDevelopment.GetExperiment("dmseismicHammer");
                        _usageMaskInt = "<b><color=red>SURFACE ONLY, handled by EVA</color></b>";
                    }
                    List<string> itemInfo = PrepareSituationAndBiomes(experiment);
                    if (PrepareInfoDescriptionSE(current, "DMSeismic", itemInfo))
                    {
                        return;
                    }
                }
            }
            finally
            {
                ((IDisposable)enumerator).Dispose();
            }
            DMagic_Present_X = true;
        }
        if (SEP_Present)
        {
            _partsWithScienceSEP = (from p in PartLoader.LoadedPartsList
                                    where p.name.Contains("SEP.")
                                    select p).ToList();
            enumerator = _partsWithScienceSEP.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    AvailablePart current2 = enumerator.Current;
                    foreach (PartModule module in current2.partPrefab.Modules)
                    {
                        if (module.moduleName == "ModuleSEPScienceExperiment")
                        {
                            ScienceExperiment experiment2 = ResearchAndDevelopment.GetExperiment(module.Fields.GetValue("experimentID") + "_Basic") ?? new ScienceExperiment();
                            List<string> itemInfo2 = PrepareSituationAndBiomes(experiment2);
                            _usageMaskInt = "<b><color=red>SURFACE ONLY, handled by EVA</color></b>";
                            if (PrepareInfoDescriptionSE(current2, "SEPScience Experiment", itemInfo2))
                            {
                                return;
                            }
                        }
                    }
                }
            }
            finally
            {
                ((IDisposable)enumerator).Dispose();
            }
        }
        _partsWithScience = (from p in PartLoader.LoadedPartsList
                             where p.partPrefab.Modules.GetModules<ModuleScienceExperiment>().Any()
                             select p).ToList();
        enumerator = _partsWithScience.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                AvailablePart current4 = enumerator.Current;
                foreach (ModuleScienceExperiment module2 in current4.partPrefab.Modules.GetModules<ModuleScienceExperiment>())
                {
                    ScienceExperiment experiment3 = ResearchAndDevelopment.GetExperiment(module2.experimentID) ?? new ScienceExperiment();
                    List<string> itemInfo3 = PrepareSituationAndBiomes(experiment3);
                    _usageMaskInt = SituationHelper.UsageMaskTemplates[module2.usageReqMaskInternal];
                    PrepareInfoDescription(current4, module2, itemInfo3);
                }
            }
        }
        finally
        {
            ((IDisposable)enumerator).Dispose();
        }
    }

    private void Impact()
    {
        if (Impact_Present && !Impact_Present_X)
        {
            _partsWithScienceIMP = (from p in PartLoader.LoadedPartsList
                                    where p.name.Contains("Impact")
                                    select p).ToList();
            foreach (AvailablePart item2 in _partsWithScienceIMP)
            {
                ScienceExperiment scienceExperiment = new ScienceExperiment();
                string name = item2.name;
                if (!(name == "Impact Seismometer"))
                {
                    if (name == "Impact Spectrometer")
                    {
                        scienceExperiment = ResearchAndDevelopment.GetExperiment("ImpactSpectrometer");
                        _usageMaskInt = "<b><color=red>ORBIT ONLY, automated</color></b>";
                    }
                }
                else
                {
                    scienceExperiment = ResearchAndDevelopment.GetExperiment("ImpactSeismometer");
                    _usageMaskInt = "<b><color=red>SURFACE ONLY, automated</color></b>";
                }
                List<string> moduleInfos = PrepareSituationAndBiomes(scienceExperiment);
                AvailablePart.ModuleInfo item = new AvailablePart.ModuleInfo
                {
                    moduleDisplayName = scienceExperiment.experimentTitle,
                    info = GetInfo(moduleInfos, _usageMaskInt)
                };
                item2.moduleInfos.Add(item);
            }
            Impact_Present_X = true;
        }
    }

    private void RoverScience()
    {
        if (ROV_Present && !ROV_Present_X)
        {
            _partsWithScienceROV = (from p in PartLoader.LoadedPartsList
                                    where p.name.Contains("roverBrain")
                                    select p).ToList();
            foreach (AvailablePart item2 in _partsWithScienceROV)
            {
                ScienceExperiment experiment = ResearchAndDevelopment.GetExperiment("RoverScienceExperiment");
                List<string> moduleInfos = PrepareSituationAndBiomes(experiment);
                _usageMaskInt = "<b><color=red>SURFACE ONLY, find a place</color></b>";
                AvailablePart.ModuleInfo item = new AvailablePart.ModuleInfo
                {
                    moduleDisplayName = experiment.experimentTitle,
                    info = GetInfo(moduleInfos, _usageMaskInt)
                };
                item2.moduleInfos.Add(item);
            }
            ROV_Present_X = true;
        }
    }

    private List<string> PrepareSituationAndBiomes(ScienceExperiment experiment)
    {
        List<string> list = new List<string>();
        foreach (KeyValuePair<ExperimentSituations, Func<bool, string>> situationTemplate in SituationHelper.SituationTemplates)
        {
            uint num = experiment.situationMask;
            if (ROV_Present && experiment.situationMask == 64)
            {
                num = 1u;
            }
            bool flag = ((int)num & (int)situationTemplate.Key) == (int)situationTemplate.Key;
            string text = situationTemplate.Value(flag);
            if (flag && ((int)experiment.biomeMask & (int)situationTemplate.Key) == (int)situationTemplate.Key)
            {
                text += _biomeDepending;
            }
            list.Add(text);
        }
        return list;
    }

    private void PrepareInfoDescription(AvailablePart part, ModuleScienceExperiment moduleScienceExperiment, List<string> itemInfo)
    {
        try
        {
            List<AvailablePart.ModuleInfo> moduleInfos = part.moduleInfos;
            AvailablePart.ModuleInfo moduleInfo = null;
            foreach (AvailablePart.ModuleInfo item in moduleInfos)
            {
                if (item.info.Contains(moduleScienceExperiment.experimentActionName))
                {
                    moduleInfo = item;
                    break;
                }
            }
            if (moduleInfo != null)
            {
                moduleInfo.info = moduleInfo.info + "\n" + GetInfo(itemInfo, _usageMaskInt);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private bool PrepareInfoDescriptionSE(AvailablePart part, string ExperimentDisplayName, List<string> itemInfo)
    {
        try
        {
            List<AvailablePart.ModuleInfo> moduleInfos = part.moduleInfos;
            AvailablePart.ModuleInfo moduleInfo = null;
            foreach (AvailablePart.ModuleInfo item in moduleInfos)
            {
                if (item.moduleDisplayName.Contains(ExperimentDisplayName))
                {
                    moduleInfo = item;
                    break;
                }
            }
            if (moduleInfo == null)
            {
                return true;
            }
            moduleInfo.info = moduleInfo.info + "\n" + GetInfo(itemInfo, _usageMaskInt);
        }
        catch (Exception arg)
        {
            Debug.Log("DennyTX: " + arg);
        }
        return false;
    }

    private string GetInfo(List<string> moduleInfos, string usageMaskInt)
    {
        string str = "--------------------------------\n";
        foreach (string moduleInfo in moduleInfos)
        {
            str += moduleInfo + "\n";
        }
        return str + ("\n" + usageMaskInt + "\n");
    }

    private void OnDestroy()
    {
        List<AvailablePart>.Enumerator enumerator = _partsWithScience.GetEnumerator();
        List<AvailablePart.ModuleInfo>.Enumerator enumerator2;
        try
        {
            while (enumerator.MoveNext())
            {
                enumerator2 = enumerator.Current.partPrefab.partInfo.moduleInfos.GetEnumerator();
                try
                {
                    while (enumerator2.MoveNext())
                    {
                        AvailablePart.ModuleInfo current = enumerator2.Current;
                        int num = current.info.IndexOf("--------------------------------", StringComparison.Ordinal);
                        if (num >= 0)
                        {
                            current.info = current.info.Remove(num - 1);
                        }
                    }
                }
                finally
                {
                    ((IDisposable)enumerator2).Dispose();
                }
            }
        }
        finally
        {
            ((IDisposable)enumerator).Dispose();
        }
        if (SEP_Present)
        {
            enumerator = _partsWithScienceSEP.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    enumerator2 = enumerator.Current.partPrefab.partInfo.moduleInfos.GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                        {
                            AvailablePart.ModuleInfo current2 = enumerator2.Current;
                            int num2 = current2.info.IndexOf("--------------------------------", StringComparison.Ordinal);
                            if (num2 >= 0)
                            {
                                current2.info = current2.info.Remove(num2 - 1);
                            }
                        }
                    }
                    finally
                    {
                        ((IDisposable)enumerator2).Dispose();
                    }
                }
            }
            finally
            {
                ((IDisposable)enumerator).Dispose();
            }
        }
    }
}
