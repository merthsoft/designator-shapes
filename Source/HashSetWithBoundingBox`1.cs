using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Merthsoft.DesignatorShapes {
    internal class HashSetWithBoundingBox<T> : IEnumerable<T> {
        private IEnumerable<T> BoundingBox { get; set; }
        private HashSet<T> Items { get; set; } = new HashSet<T>();

        public int Count => Items.Count;
        public bool IsReadOnly => false;

        public HashSetWithBoundingBox(IEnumerable<T> boundingBox) {
            BoundingBox = boundingBox;
        }

        public IEnumerator<T> GetEnumerator()
            => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => (Items as IEnumerable).GetEnumerator();

        public void Add(T item) {
            if (BoundingBox.Contains(item)) {
                Items.Add(item);
            }
        }

        public void AddRange(IEnumerable<T> items)
            => items.ForEach(item => Add(item));
    }
}
