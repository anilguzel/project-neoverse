CREATE DATABASE customers;
CREATE DATABASE docs;

\connect customers;

CREATE TABLE "Customers" (
                             "Id" UUID PRIMARY KEY,
                             "Name" TEXT NOT NULL,
                             "Email" TEXT NOT NULL,
                             "CreatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                             "UpdatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                             "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
                             "TenantId" UUID
);

CREATE TABLE "Translations" (
                                "Id" UUID PRIMARY KEY,
                                "EntityId" UUID NOT NULL,
                                "EntityType" TEXT NOT NULL,
                                "PropertyName" TEXT NOT NULL,
                                "Language" TEXT NOT NULL,
                                "Value" TEXT NOT NULL,
                                CONSTRAINT fk_translation_customer FOREIGN KEY ("EntityId") REFERENCES "Customers" ("Id") ON DELETE CASCADE
);

CREATE INDEX idx_translation_lookup_customer ON "Translations" ("EntityId", "PropertyName", "Language");

\connect docs;

CREATE TABLE "Documents" (
                             "Id" UUID PRIMARY KEY,
                             "Title" TEXT NOT NULL,
                             "CustomerId" UUID NOT NULL,
                             "CreatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                             "UpdatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                             "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
                             "TenantId" UUID
);

CREATE TABLE "Translations" (
                                "Id" UUID PRIMARY KEY,
                                "EntityId" UUID NOT NULL,
                                "EntityType" TEXT NOT NULL,
                                "PropertyName" TEXT NOT NULL,
                                "Language" TEXT NOT NULL,
                                "Value" TEXT NOT NULL,
                                CONSTRAINT fk_translation_document FOREIGN KEY ("EntityId") REFERENCES "Documents" ("Id") ON DELETE CASCADE
);

CREATE INDEX idx_translation_lookup_document ON "Translations" ("EntityId", "PropertyName", "Language");
