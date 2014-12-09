using Concepts.Ring1;
using Concepts.Ring3;
using Starcounter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Concepts.Ring8.Polyjuice {

    [Database]
    public class SystemUserGroupBasedOn : Relation {

        /// <summary>
        /// A SystemUserGroup based on another SystemUserGroup
        /// </summary>
        [SynonymousTo("WhatIs")]
        public readonly SystemUserGroup SystemUserGroup;
        public void SetSystemUserGroup(SystemUserGroup SystemUserGroup) {
            SetWhatIs(SystemUserGroup);
        }

        /// <summary>
        /// 
        /// </summary>
        [SynonymousTo("ToWhat")]
        public readonly SystemUserGroup SystemUserGroupBaseOn;
    }
}
