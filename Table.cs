        public static class Table
        {
            /// <summary>
            /// Fills the public properties of a class from the first row of a DataTable
            ///  where the name of the property matches the column name from that DataTable.
            /// </summary>
            /// <param name="table">A DataTable that contains the data.</param>
            /// <returns>A class of type T with its public properties matching column names
            ///      set to the values from the first row in the DataTable.</returns>
            public static T ToClass<T>(DataTable table) where T : class, new()
            {
                T result = new T();
                if (Validate(table))
                {  // Because reflection is slow, we will only pass the first row of the DataTable
                    result = FillProperties<T>(table.Rows[0]);
                }
                return result;
            }

            /// <summary>
            /// Fills the public properties of a class from each row of a DataTable where the name of
            /// the property matches the column name in the DataTable, returning a List of T.
            /// </summary>
            /// <param name="table">A DataTable that contains the data.</param>
            /// <returns>A List class T with each class's public properties matching column names
            ///      set to the values of a different row in the DataTable.</returns>
            public static List<T> ToClassList<T>(DataTable table) where T : class, new()
            {
                List<T> result = new List<T>();

                if (Validate(table))
                {
                    foreach (DataRow row in table.Rows)
                    {
                        result.Add(FillProperties<T>(row));
                    }
                }
                return result;
            }

            /// <summary>
            /// Fills the public properties of a class from a DataRow where the name
            /// of the property matches a column name from that DataRow.
            /// </summary>
            /// <param name="row">A DataRow that contains the data.</param>
            /// <returns>A class of type T with its public properties set to the
            ///      data from the matching columns in the DataRow.</returns>
            public static T FillProperties<T>(DataRow row) where T : class, new()
            {
                T result = new T();
                Type classType = typeof(T);

                // Defensive programming, make sure there are properties to set,
                //   and columns to set from and values to set from.
                if (row.Table.Columns.Count < 1
                    || classType.GetProperties().Length < 1
                    || row.ItemArray.Length < 1)
                {
                    return result;
                }

                foreach (PropertyInfo property in classType.GetProperties())
                {
                    foreach (DataColumn column in row.Table.Columns)
                    {
                        // Skip if Property name and ColumnName do not match
                        if (property.Name != column.ColumnName)
                            continue;
                        // This would throw if we tried to convert it below
                        if (row[column] == DBNull.Value)
                            continue;

                        object newValue;

                        // If type is of type System.Nullable, do not attempt to convert the value
                        if (IsNullable(property.PropertyType))
                        {
                            newValue = row[property.Name];
                        }
                        else
                        {   // Convert row object to type of property
                            newValue = Convert.ChangeType(row[column], property.PropertyType);
                        }

                        // This is what sets the class properties of the class
                        property.SetValue(result, newValue, null);
                    }
                }
                return result;
            }

            /// <summary>
            /// Checks a DataTable for empty rows, columns or null.
            /// </summary>
            /// <param name="dataTable">The DataTable to check.</param>
            /// <returns>True if DataTable has data, false if empty or null.</returns>
            public static bool Validate(DataTable dataTable)
            {
                if (dataTable == null) return false;
                if (dataTable.Rows.Count == 0) return false;
                if (dataTable.Columns.Count == 0) return false;
                return true;
            }

            /// <summary>
            /// Checks if type is nullable, Nullable<T> or its reference is nullable.
            /// </summary>
            /// <param name="type">Type to check for nullable.</param>
            /// <returns>True if type is nullable, false if it is not.</returns>
            public static bool IsNullable(Type type)
            {
                if (!type.IsValueType) return true; // ref-type
                if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
                return false; // value-type
            }
        }
