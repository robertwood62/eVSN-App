namespace Fri.FieldPlotService.Tools
{ 
    /// <summary>
    /// Provides a list of field plot error codes back to the client application.
    /// </summary>
    public enum FieldPlotServiceError
    {
        /// <summary>
        /// A general error has occurred.
        /// </summary>
        GeneralError = -1,

        /// <summary>
        /// Indicates there is a service configuration error.
        /// </summary>
        ConfigurationError = 0,

        /// <summary>
        /// The table being requested is not available for JSON based inserts.
        /// </summary>
        TableAccessDenied = 1,

        /// <summary>
        /// The stored procedure failed to update the records in the table.
        /// </summary>
        TableUpdateError = 3,

        /// <summary>
        /// The stored procedure failed to insert the records into the table.
        /// </summary>
        TableInsertError = 4,

        /// <summary>
        /// Indicate that an entity is missing the required ID field.
        /// </summary>
        TableDataIdMissing = 5,

        /// <summary>
        /// The table already contains a record with the same ID.
        /// </summary>
        TableDataExists = 6,

        /// <summary>
        /// The table does not contain an existing record with the ID provided.
        /// </summary>
        TableDataNotFound = 7,

        /// <summary>
        /// The username is invalid.
        /// </summary>
        UsernameInvalid = 8,

        /// <summary>
        /// The user is currently blocked from using the system.
        /// </summary>
        VendorUserBlocked = 11,

        /// <summary>
        /// The vendor is currently blocked from using the system.
        /// </summary>
        VendorBlocked = 12,

        /// <summary>
        /// The vendor was not found.
        /// </summary>
        VendorNotFound = 13,

        /// <summary>
        /// A duplicate username already exists in the system.
        /// </summary>
        UsernameExists = 14,

        /// <summary>
        /// The vendor user was not found.
        /// </summary>
        VendorUserNotFound = 15,

        /// <summary>
        /// The vendor's name is invalid.
        /// </summary>
        VendorNameInvalid = 16,

        /// <summary>
        /// The vendor code is invalid.
        /// </summary>
        VendorCodeInvalid = 17,

        /// <summary>
        /// Indicates a vendor with the same code already exists in the database.
        /// </summary>
        VendorAlreadyExists = 18,

        /// <summary>
        /// Vendor deletes are blocked if the vendor has projects in the system.  These must be removed first.
        /// </summary>
        VendorDeleteFailed = 19,

        /// <summary>
        /// The VSN Entity cannot be null.
        /// </summary>
        VSNEntityCannotBeNull = 20,

        /// <summary>
        /// The Plot Name is missing.
        /// </summary>
        VSNPlotNameInvalid = 21,

        /// <summary>
        /// The Deformity ID is missing.
        /// </summary>
        VSNDeformityIdInvalid = 22,

        /// <summary>
        /// The Tree ID is missing.
        /// </summary>
        VSNTreeIdInvalid = 23,

        /// <summary>
        /// The Project ID is missing.
        /// </summary>
        VSNProjectIdInvalid = 24,

        /// <summary>
        /// The Plot ID is missing.
        /// </summary>
        VSNPlotIdInvalid = 25,

        /// <summary>
        /// The Dwd ID is missing.
        /// </summary>
        VSNDwdIdInvalid = 26,

        /// <summary>
        /// The Ecosite ID is missing.
        /// </summary>
        VSNEcositeIdInvalid = 27,

        /// <summary>
        /// The Person ID is missing.
        /// </summary>
        VSNPersonIdInvalid = 28,

        /// <summary>
        /// The Photo ID is missing.
        /// </summary>
        VSNPhotoIdInvalid = 29,

        /// <summary>
        /// The Small Tree ID is missing.
        /// </summary>
        VSNSmallTreeIdInvalid = 30,

        /// <summary>
        /// The Project Name is missing.
        /// </summary>
        VSNProjectNameInvalid = 31,

        /// <summary>
        /// The Small Tree Tally ID is missing.
        /// </summary>
        VSNSmallTreeTallyIdInvalid = 32,

        /// <summary>
        /// The Soil ID is missing.
        /// </summary>
        VSNSoilIdInvalid = 33,

        /// <summary>
        /// The Stem Map ID is missing.
        /// </summary>
        VSNStemMapIdInvalid = 34,

        /// <summary>
        /// The Vegetation ID is missing.
        /// </summary>
        VSNVegetationIdInvalid = 35,

        /// <summary>
        /// The Vegetation Census ID is missing.
        /// </summary>
        VSNVegetationCensusIdInvalid = 36,
        
        /// <summary>
        /// The user information was not provided.
        /// </summary>
        UserInfoMissing = 37,

        /// <summary>
        /// The password is missing.
        /// </summary>
        PasswordMissing = 38,

        /// <summary>
        /// The project information is missing.
        /// </summary>
        ProjectInfoMissing = 39,

        /// <summary>
        /// The project ID already exists.
        /// </summary>
        VsnProjectIdAlreadyExists = 40,

        /// <summary>
        /// The project was not found.
        /// </summary>
        ProjectNotFound = 41,

        /// <summary>
        /// The VSN Plot name already exists (this must be unique within the entire system).
        /// </summary>
        VSNPlotNameAlreadyExists = 42,

        /// <summary>
        /// The plot was not found.
        /// </summary>
        PlotNotFound = 43
    }

    /// <summary>
    /// Exception for all field plot service related errors.
    /// </summary>
    public class FieldPlotServiceException : Exception
    {
        /// <summary>
        /// Constructs the field plot error message.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public FieldPlotServiceException(FieldPlotServiceError errorCode, string? message = null, Exception? innerException = null)
            :base(message ?? $"Field plot service error code: {errorCode}", innerException)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// The field plot service error code.
        /// </summary>
        public FieldPlotServiceError ErrorCode { get; protected set; }
    }
}
