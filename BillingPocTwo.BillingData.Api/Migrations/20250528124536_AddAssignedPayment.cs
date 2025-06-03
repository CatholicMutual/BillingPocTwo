using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingPocTwo.BillingData.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ENTITY_ADDRESS_INFO",
                columns: table => new
                {
                    SEQ_ENTITY_ADDRESS_INFO = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SYSTEM_ENTITY_CODE = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ADDRESS_TYPE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ADDRESS1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ADDRESS2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CITY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZIP_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FULL_NAME = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENTITY_ADDRESS_INFO", x => x.SEQ_ENTITY_ADDRESS_INFO);
                });

            migrationBuilder.CreateTable(
                name: "ENTITY_REGISTER",
                columns: table => new
                {
                    SYSTEM_ENTITY_CODE = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DOING_BUSINESS_AS_NAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ENTITY_TYPE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SOURCE_SYSTEM_ENTITY_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BALANCE = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENTITY_REGISTER", x => x.SYSTEM_ENTITY_CODE);
                });

            migrationBuilder.CreateTable(
                name: "INT_BLNG_INQ_INV_DTL",
                columns: table => new
                {
                    AUTO_KEY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BILLING_INQ_REQ_SEQ = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ID_KEY = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    REQ_SEQ_SERIAL_NO = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    REQ_SEQ_SUB_SERIAL_NO = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_AMT_AFTER_LAST_INV = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BALANCE = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CURRENT_MIN_DUE = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LAST_INVOICE_AMT = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LAST_INVOICE_DATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LAST_INVOICE_DUE_DATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NEXT_INST_DATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NEXT_INST_DUE_AMT = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NEXT_INS_DUE_DATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PAST_DUE_AMT = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PAY_RECVD_AFT_LAST_INV = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PLEASE_PAY_AMT = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TOTAL_ADJUSTMENT_AMT = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TOTAL_PAYMENT_AMT = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TOTAL_RECV_AMT = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    POLICY_TERM_ID = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ACCOUNT_SYSTEM_CODE = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INT_BLNG_INQ_INV_DTL", x => x.AUTO_KEY);
                });

            migrationBuilder.CreateTable(
                name: "POLICY_ENTITY_REGISTER",
                columns: table => new
                {
                    POLICY_TERM_ID = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SYSTEM_ENTITY_CODE = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SYSTEM_ACTIVITY_NO = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SYSTEM_TRANSACTION_SEQ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ENTITY_TYPE = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ENTITY_TRANSACTION_SEQ = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BILLING_ENTITY_YN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LOCATION_ID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POLICY_ENTITY_REGISTER", x => new { x.POLICY_TERM_ID, x.ENTITY_TYPE, x.SYSTEM_ENTITY_CODE, x.SYSTEM_ACTIVITY_NO, x.SYSTEM_TRANSACTION_SEQ });
                });

            migrationBuilder.CreateTable(
                name: "POLICY_REGISTER",
                columns: table => new
                {
                    POLICY_TERM_ID = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    POLICY_NO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POLICY_RENEW_NO = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    INBOUND_TRANSACTION_SEQ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SYSTEM_TRANSACTION_SEQ = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PRODUCT_CODE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    STATE_CODE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OPERATING_COMPANY = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CRT_CODE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ACCOUNT_SYSTEM_CODE = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BROKER_SYSTEM_CODE = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    INSURED_SYSTEM_CODE = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PAYMENT_PLAN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    APPLICATION_NO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    POLICY_EFFECTIVE_DATE = table.Column<DateTime>(type: "datetime2", nullable: false),
                    POLICY_EXPIRATION_DATE = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LEGAL_STATUS = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BILL_TO_SYSTEM_CODE = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    POLICY_ID = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EQUITY_DATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    COUNTRY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SOURCE_POLICY_ID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BALANCE = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ROWID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SUB_BILLTYPE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SYSTEM_STATUS = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POLICY_REGISTER", x => x.POLICY_TERM_ID);
                });

            migrationBuilder.CreateTable(
                name: "TRANSACTION_LOG",
                columns: table => new
                {
                    SYSTEM_TRANSACTION_SEQ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    POLICY_TERM_ID = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    POLICY_NO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TRANSACTION_TYPE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CREATED_ON = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TRANSACTION_EFF_DATE = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TRANSACTION_EXPIRY_DATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CREATED_BY = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRANSACTION_LOG", x => x.SYSTEM_TRANSACTION_SEQ);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ENTITY_ADDRESS_INFO");

            migrationBuilder.DropTable(
                name: "ENTITY_REGISTER");

            migrationBuilder.DropTable(
                name: "INT_BLNG_INQ_INV_DTL");

            migrationBuilder.DropTable(
                name: "POLICY_ENTITY_REGISTER");

            migrationBuilder.DropTable(
                name: "POLICY_REGISTER");

            migrationBuilder.DropTable(
                name: "TRANSACTION_LOG");
        }
    }
}
