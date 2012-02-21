using System;
using System.Collections;

namespace MailDeluxe.Imap {
    public class Quota {
        private string ressource;
        private string usage;
        private int used;
        private int max;
        public Quota(string ressourceName, string usage, int used, int max) {
            this.ressource = ressourceName;
            this.usage = usage;
            this.used = used;
            this.max = max;
        }
        public int Used {
            get { return this.used; }
        }
        public int Max {
            get { return this.max; }
        }
    }
}