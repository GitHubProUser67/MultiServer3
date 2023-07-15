namespace PSMultiServer.SRC_Addons.MEDIUS.RT.Common
{
    public static class Extensions
    {
        #region MediusUniverseVariableInformationInfoFilter

        public static bool IsSet(this MediusUniverseVariableInformationInfoFilter value, MediusUniverseVariableInformationInfoFilter filter)
        {
            return (value & filter) == filter;
        }

        #endregion

        #region MediusComparisonOperator

        public static bool Compare(this MediusComparisonOperator op, ulong lhs, ulong rhs)
        {
            switch (op)
            {
                case MediusComparisonOperator.EQUAL_TO: return lhs == rhs;
                case MediusComparisonOperator.GREATER_THAN: return lhs > rhs;
                case MediusComparisonOperator.GREATER_THAN_OR_EQUAL_TO: return lhs >= rhs;
                case MediusComparisonOperator.LESS_THAN: return lhs < rhs;
                case MediusComparisonOperator.LESS_THAN_OR_EQUAL_TO: return lhs <= rhs;
                case MediusComparisonOperator.NOT_EQUALS: return lhs != rhs;
                default: return false;
            }
        }

        #endregion

        #region MediusLobbyFilter

        public static bool Compare(this MediusLobbyFilterType ft, ulong lhs, ulong rhs)
        {
            switch (ft)
            {
                case MediusLobbyFilterType.MediusLobbyFilterEqualsLobby: return true;
                case MediusLobbyFilterType.MediusLobbyFilterEqualsFilter: return lhs == rhs;
                case MediusLobbyFilterType.FILTER_EQUALS: return lhs == rhs;
                default: return false;
            }
        }

        #endregion
    }
}
