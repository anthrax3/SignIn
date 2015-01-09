using Concepts.Ring1;
using Concepts.Ring3;
using Starcounter;

namespace Concepts.Ring8.Polyjuice.Permissions {

    [Database]
    public class SystemUserGroupUriPermission : Relation {

        [SynonymousTo("WhatIs")]
        public readonly SystemUserGroup SystemUserGroup;

        [SynonymousTo("ToWhat")]
        public readonly UriPermission Permission;
    }
}
