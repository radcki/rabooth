CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Collages` (
    `CollageId` char(36) COLLATE ascii_general_ci NOT NULL,
    `CaptureDateTime` datetime(6) NOT NULL,
    `AddedDateTime` datetime(6) NOT NULL,
    `CollagePhoto_PhotoId` char(36) COLLATE ascii_general_ci NOT NULL,
    `CollagePhoto_Index` int NOT NULL,
    `CollagePhoto_CaptureDateTime` datetime(6) NOT NULL,
    `CollagePhoto_AddedDateTime` datetime(6) NOT NULL,
    `CollagePhoto_Deleted` tinyint(1) NOT NULL,
    `CollagePhoto_DeletedDate` datetime(6) NULL,
    `Deleted` tinyint(1) NOT NULL,
    `DeletedDate` datetime(6) NULL,
    CONSTRAINT `PK_Collages` PRIMARY KEY (`CollageId`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `CollageSourcePhoto` (
    `CollageSourcePhotoId` int NOT NULL AUTO_INCREMENT,
    `PhotoId` char(36) COLLATE ascii_general_ci NOT NULL,
    `CollageId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Index` int NOT NULL,
    `CaptureDateTime` datetime(6) NOT NULL,
    `AddedDateTime` datetime(6) NOT NULL,
    `Deleted` tinyint(1) NOT NULL,
    `DeletedDate` datetime(6) NULL,
    CONSTRAINT `PK_CollageSourcePhoto` PRIMARY KEY (`CollageSourcePhotoId`),
    CONSTRAINT `FK_CollageSourcePhoto_Collages_CollageId` FOREIGN KEY (`CollageId`) REFERENCES `Collages` (`CollageId`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_CollageSourcePhoto_CollageId` ON `CollageSourcePhoto` (`CollageId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20240602115645_InitialCreate', '8.0.6');

COMMIT;

