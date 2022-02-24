using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Abstractions.Entities
{
    public class ModuleModel
    {
        /// <summary>
        /// id值
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        public string PrefixSpace { get; set; }

        /// <summary>
        /// 模块地址
        /// </summary>
        public string ModulePath
        {
            get
            {
                return $"{Id}_{Name}";
            }
        }
    }
}
