
    using System.Collections.Generic;
    using System.Text;
    using DebugUtil = Library.DebugUtil;

    public class TStatContainer<T>
    {
        private Dictionary<T,List<TStatModifier<T>>> _mods = new Dictionary<T, List<TStatModifier<T>>>();

        private Dictionary<T, float> _cacheStats = new Dictionary<T, float>();

        public Dictionary<T, float> Stats => _cacheStats;

        private StringBuilder _sb;
        
        public void Initialize()
        {
            foreach (var list in _mods.Values)
            {
                foreach (var modifier in list)
                {
                    modifier.Dispose();
                }
                
                list.Clear();
            }
        }
        
        public void CalculateStat()
        {
            _cacheStats.Clear();
            
            foreach (var mod in _mods)
            {
                var type = mod.Key;
                var list = mod.Value;

                var value = 0f;
                
                foreach (var modifier in list)
                {
                    if (modifier.ModifierType != eModifierType.Additive)
                    {
                        continue;
                    }

                    value += modifier.Value;
                }
                
                foreach (var modifier in list)
                {
                    if (modifier.ModifierType != eModifierType.Percent)
                    {
                        continue;
                    }

                    value *= (1 + modifier.Value);
                }

                if (!_cacheStats.TryGetValue(type, out var key))
                {
                    _cacheStats.Add(type,value);
                }
                else
                {
                    _cacheStats[type] += value;
                }
            }
        }
        
        public float GetStatValue(T type)
        {
            if (_cacheStats.TryGetValue(type, out var value))
            {
                return value;
            }

            DebugUtil.LogWarning($"Stat {type} not initialized.");

            return 0;
        }
        public void AddModifier(TStatModifier<T> m, bool calculate = false)
        {
            if (!_mods.TryGetValue(m.StatType, out var list))
            {
                list = new List<TStatModifier<T>>();
                
                _mods.Add(m.StatType,list);
            }

            var existModifier = list.Find(x => x.ModifierId == m.ModifierId);
            
            if (existModifier != null)
            {
                list.Remove(existModifier);
                
                existModifier.Dispose();
            }

            list.Add(m);

            if (calculate)
            {
                CalculateStat();
            }
        }
        
        public void RemoveModifier(TStatModifier<T> m, bool calculate = false)
        {
            if (!_mods.TryGetValue(m.StatType, out var list))
            {
                return;
            }

            list.Remove(m);
            
            if (calculate)
            {
                CalculateStat();
            }
        }

        public override string ToString()
        {
            _sb ??= new StringBuilder();
            _sb.Clear();
            
            foreach (var stat in _cacheStats)
            {
                _sb.Append($"{stat.Key} : {stat.Value}\n");
            }

            return _sb.ToString();
        }
    }

    public enum eModifierType
    {
        Additive,
        Percent,
    }
