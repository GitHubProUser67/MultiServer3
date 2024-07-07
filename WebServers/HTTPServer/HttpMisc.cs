namespace HTTPServer
{
    public static class HttpMisc
    {
        /// <summary>
        /// Add a dynamic array to a existing array structure.
        /// <para>Ajoute un objet dynamique sur une structure existante.</para>
        /// </summary>
        /// <param name="jaggedArray">A complex array.</param>
        /// <param name="newElement">the complex element to add.</param>
        /// <returns>A complex jagguedArray.</returns>
        public static T[][] AddElementToLastPosition<T>(T[][] jaggedArray, T[] newElement)
        {
            // Create a new jagged array with increased size
            T[][] newArray = new T[jaggedArray.Length + 1][];

            // Copy existing elements to the new array
            for (int i = 0; i < jaggedArray.Length; i++)
            {
                newArray[i] = jaggedArray[i];
            }

            // Add the new element to the last position
            newArray[^1] = newElement;

            return newArray;
        }

        /// <summary>
        /// Add multiple dynamic arrays to an existing array structure.
        /// <para>Ajoute plusieurs objets dynamiques sur une structure existante.</para>
        /// </summary>
        /// <typeparam name="T">Type of the elements in the arrays.</typeparam>
        /// <param name="jaggedArray">A complex array.</param>
        /// <param name="newElements">The complex elements to add.</param>
        /// <returns>A complex jagged array.</returns>
        public static T[][] AddElementsToLastPosition<T>(T[][] jaggedArray, params T[][] newElements)
        {
            // Create a new jagged array with increased size
            T[][] newArray = new T[jaggedArray.Length + newElements.Length][];

            // Copy existing elements to the new array
            for (int i = 0; i < jaggedArray.Length; i++)
            {
                newArray[i] = jaggedArray[i];
            }

            // Add the new elements to the last positions
            for (int i = 0; i < newElements.Length; i++)
            {
                newArray[jaggedArray.Length + i] = newElements[i];
            }

            return newArray;
        }
    }
}
