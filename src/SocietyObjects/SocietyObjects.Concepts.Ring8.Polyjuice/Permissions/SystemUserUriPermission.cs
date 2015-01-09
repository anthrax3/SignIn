using Concepts.Ring1;
using Concepts.Ring3;
using Starcounter;

namespace Concepts.Ring8.Polyjuice.Permissions {

    [Database]
    public class SystemUserUriPermission : Relation {

        [SynonymousTo("WhatIs")]
        public readonly SystemUser SystemUser;

        [SynonymousTo("ToWhat")]
        public readonly UriPermission Permission;
    }

}
