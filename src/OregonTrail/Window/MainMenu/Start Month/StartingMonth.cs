// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

namespace OregonTrail
{
    /// <summary>
    ///     Special enumeration used for defining the starting month of the game simulation. Since we want to user to select
    ///     one through five from March to July we need a special way to keep track of what months are valid for starting and
    ///     have them in selectable order that makes sense to the user.
    /// </summary>
    public enum StartingMonth
    {
        /// <summary>
        ///     Month of March.
        /// </summary>
        March = 1,

        /// <summary>
        ///     Month of April.
        /// </summary>
        April = 2,

        /// <summary>
        ///     Month of May.
        /// </summary>
        May = 3,

        /// <summary>
        ///     Month of June.
        /// </summary>
        June = 4,

        /// <summary>
        ///     Month of July.
        /// </summary>
        July = 5
    }
}