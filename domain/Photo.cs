using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotoWeasel.domain {
	public class Photo {
		public int ID { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public DateTime Date { get; set; }
		public byte[] Image { get; set; }
        public byte[] Thumbnail { get; set; }
	}
}
