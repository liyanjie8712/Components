﻿using System.Collections.Generic;

namespace Liyanjie.ComplexTypes
{
    /// <summary>
    /// 地址
    /// </summary>
    public class Address : ValueObject
    {
        /// <summary>
        /// 行政区划代码
        /// </summary>
        public string ADCode { get; set; }

        /// <summary>
        /// 行政区划详细
        /// </summary>
        public string ADDetail { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return ADCode;
            yield return ADDetail;
            yield return Detail;
        }
    }
}
