using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace orgBidAplctn.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "COMM_COMP_INFO",
                columns: table => new
                {
                    COMP_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    COMP_NAME = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ADDRESS = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    COMP_AREA = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    COMP_DIST = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    COMP_COUNTRY = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    POST_CODE = table.Column<int>(type: "int", nullable: true),
                    BIN_NO = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    VAT_PRC = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValueSql: "((0))"),
                    CONT_PERSN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CONT_EMAIL = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CONT_SMS_NO = table.Column<string>(type: "char(14)", unicode: false, fixedLength: true, maxLength: 14, nullable: true),
                    CONT_OTH_NO = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DEF_CURR = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false, defaultValueSql: "('BDT')"),
                    COMP_STRT_NO = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    DSCNTND = table.Column<byte>(type: "tinyint", nullable: false),
                    LOGO_FILE_NM = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    LOGO_DB = table.Column<byte[]>(type: "image", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMM_COMP_INFO", x => x.COMP_ID);
                });

            migrationBuilder.CreateTable(
                name: "COMM_COUNTRY_INFO",
                columns: table => new
                {
                    CNTRY_ID = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    CNTRY_NAME = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMM_COUNTRY_INFO", x => x.CNTRY_ID);
                });

            migrationBuilder.CreateTable(
                name: "COMM_PARTY_CAT",
                columns: table => new
                {
                    CAT_ID = table.Column<string>(type: "char(12)", unicode: false, fixedLength: true, maxLength: 12, nullable: false),
                    CAT_NAME = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    IS_DISCONTINUE = table.Column<byte>(type: "tinyint", nullable: false),
                    COMP_ID = table.Column<int>(type: "int", nullable: false),
                    ADD_BY = table.Column<long>(type: "bigint", nullable: true),
                    CREATE_TM = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMM_PARTY_CAT", x => x.CAT_ID);
                });

            migrationBuilder.CreateTable(
                name: "COMM_PROD_CAT",
                columns: table => new
                {
                    CAT_ID = table.Column<string>(type: "char(12)", unicode: false, fixedLength: true, maxLength: 12, nullable: false),
                    CAT_NAME = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BASE_UNIT = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    IS_INV_ITEM = table.Column<byte>(type: "tinyint", nullable: true, defaultValueSql: "((1))"),
                    IS_DISCONTINUE = table.Column<byte>(type: "tinyint", nullable: false),
                    COMP_ID = table.Column<int>(type: "int", nullable: false),
                    ADD_BY = table.Column<long>(type: "bigint", nullable: false),
                    CREATE_TM = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMM_PROD_CAT", x => x.CAT_ID);
                });

            migrationBuilder.CreateTable(
                name: "COMM_SMS_LOG",
                columns: table => new
                {
                    AUTO_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SMS_PHONE = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    SMS_DT_TM = table.Column<DateTime>(type: "datetime", nullable: true),
                    SMS_MSG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SMS_FROM = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: true),
                    REF_ID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValueSql: "((0))"),
                    PART_NO = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    SMS_TYPE = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: true, defaultValueSql: "('A')", comment: "A=Attendance, S=Survey, W=Web Sales"),
                    SMS_CONTAIN = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: true),
                    SMS_STAT = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: true, defaultValueSql: "('U')"),
                    SMS_UPL_STAT = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: true, defaultValueSql: "('N')"),
                    SMS_VALIDITY = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: true, defaultValueSql: "('V')", comment: "Valid Or Invalid Message")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMM_SMS_LOG", x => x.AUTO_ID);
                });

            migrationBuilder.CreateTable(
                name: "COMM_SMS_SETT",
                columns: table => new
                {
                    SYS_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TOKEN_NO = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    SENDER_NO = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    RECEIVE_NO = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMM_SMS_SETT", x => x.SYS_ID);
                });

            migrationBuilder.CreateTable(
                name: "GetVoucherIdPKs",
                columns: table => new
                {
                    VoucherId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "COMM_LOGIN_INFO",
                columns: table => new
                {
                    USER_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    USER_NM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    USER_PASS = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    FAST_NAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LAST_NAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GENDER = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: true),
                    BRTH_DT = table.Column<DateTime>(type: "datetime", nullable: true),
                    USER_ADD = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    USER_CITY = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    USER_CNTRY = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EMAIL_ADD = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CONT_NO = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    LOGIN_TP = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((1))", comment: "1 for User, 2 for Manager, 3 for Admin"),
                    CAN_MOD = table.Column<byte>(type: "tinyint", nullable: true, defaultValueSql: "((0))"),
                    CAN_DEL = table.Column<byte>(type: "tinyint", nullable: true, defaultValueSql: "((0))"),
                    IS_ACTIVE = table.Column<byte>(type: "tinyint", nullable: false, defaultValueSql: "((1))"),
                    PROFILE_PIC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    COMP_ID = table.Column<int>(type: "int", nullable: false),
                    LAST_LOG_TM = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMM_LOGIN_INFO", x => x.USER_ID);
                    table.ForeignKey(
                        name: "FK_COMM_LOGIN_INFO_COMM_COMP_INFO",
                        column: x => x.COMP_ID,
                        principalTable: "COMM_COMP_INFO",
                        principalColumn: "COMP_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "COMM_WAREHS_INFO",
                columns: table => new
                {
                    WARE_ID = table.Column<string>(type: "char(12)", unicode: false, fixedLength: true, maxLength: 12, nullable: false),
                    WARE_NAME = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WARE_ADD = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IS_DISCONTINUE = table.Column<byte>(type: "tinyint", nullable: false),
                    COMP_ID = table.Column<int>(type: "int", nullable: false),
                    ADD_BY = table.Column<long>(type: "bigint", nullable: true),
                    CREATE_TM = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMM_WAREHS_INFO", x => x.WARE_ID);
                    table.ForeignKey(
                        name: "FK_COMM_WAREHS_INFO_COMM_COMP_INFO",
                        column: x => x.COMP_ID,
                        principalTable: "COMM_COMP_INFO",
                        principalColumn: "COMP_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "COMM_PARTY_INFO",
                columns: table => new
                {
                    PARTY_ID = table.Column<string>(type: "char(12)", unicode: false, fixedLength: true, maxLength: 12, nullable: false),
                    PARTY_NAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PARTY_CODE = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: true),
                    PARTY_ADD = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DIST_NM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CNTRY_NM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CAT_ID = table.Column<string>(type: "char(12)", unicode: false, fixedLength: true, maxLength: 12, nullable: false),
                    SMS_CONT_NO = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    OTH_CONT_NO = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    EMAIL_ADD = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DOB = table.Column<DateTime>(type: "datetime", nullable: true),
                    CUST_TYPE = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValueSql: "('C')"),
                    IS_DISCONTINUE = table.Column<byte>(type: "tinyint", nullable: false),
                    IMG_FILE_NM = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    COMP_ID = table.Column<int>(type: "int", nullable: false),
                    ADD_BY = table.Column<long>(type: "bigint", nullable: true),
                    CREATE_TM = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMM_PARTY_INFO", x => x.PARTY_ID);
                    table.ForeignKey(
                        name: "FK_COMM_PARTY_INFO_COMM_COMP_INFO",
                        column: x => x.COMP_ID,
                        principalTable: "COMM_COMP_INFO",
                        principalColumn: "COMP_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_COMM_PARTY_INFO_COMM_PARTY_CAT",
                        column: x => x.CAT_ID,
                        principalTable: "COMM_PARTY_CAT",
                        principalColumn: "CAT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "COMM_PROD_INFO",
                columns: table => new
                {
                    PROD_ID = table.Column<string>(type: "char(12)", unicode: false, fixedLength: true, maxLength: 12, nullable: false),
                    PROD_NAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PROD_CODE = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: true),
                    BARCD_ID = table.Column<string>(type: "varchar(25)", unicode: false, maxLength: 25, nullable: true),
                    CAT_ID = table.Column<string>(type: "char(12)", unicode: false, fixedLength: true, maxLength: 12, nullable: false),
                    WGHT = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    PROD_SPEC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LEAD_TM = table.Column<int>(type: "int", nullable: false),
                    MIN_QNTY = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    MAX_QNTY = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    IS_DISCONTINUE = table.Column<byte>(type: "tinyint", nullable: false),
                    IMG_FILE_NM = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    COMP_ID = table.Column<int>(type: "int", nullable: false),
                    ADD_BY = table.Column<long>(type: "bigint", nullable: false),
                    ENTRY_TM = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMM_PROD_INFO", x => x.PROD_ID);
                    table.ForeignKey(
                        name: "FK_COMM_PROD_INFO_COMM_COMP_INFO",
                        column: x => x.COMP_ID,
                        principalTable: "COMM_COMP_INFO",
                        principalColumn: "COMP_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_COMM_PROD_INFO_COMM_PROD_CAT",
                        column: x => x.CAT_ID,
                        principalTable: "COMM_PROD_CAT",
                        principalColumn: "CAT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "COMM_NOTIFY_LIST",
                columns: table => new
                {
                    NOT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NOT_BY = table.Column<long>(type: "bigint", nullable: false),
                    NOT_MSG = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IS_READ = table.Column<byte>(type: "tinyint", nullable: true, defaultValueSql: "((0))"),
                    NOT_TP = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((0))"),
                    CREAT_TM = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMM_NOTIFY_LIST", x => x.NOT_ID);
                    table.ForeignKey(
                        name: "FK_COMM_NOTIFY_LIST_COMM_LOGIN_INFO_NOT_BY",
                        column: x => x.NOT_BY,
                        principalTable: "COMM_LOGIN_INFO",
                        principalColumn: "USER_ID");
                });

            migrationBuilder.CreateTable(
                name: "COMM_BID_MSTR",
                columns: table => new
                {
                    BID_ID = table.Column<long>(type: "bigint", nullable: false),
                    BID_DATE = table.Column<DateTime>(type: "datetime", nullable: false),
                    PROD_ID = table.Column<string>(type: "char(12)", unicode: false, fixedLength: true, maxLength: 12, nullable: false),
                    BID_QNTY = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    BID_RATE = table.Column<decimal>(type: "money", nullable: false),
                    ALLOC_TIME = table.Column<DateTime>(type: "datetime", nullable: true),
                    ALLOC_QNTY = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValueSql: "((0))"),
                    ALLOC_RATE = table.Column<decimal>(type: "money", nullable: true, defaultValueSql: "((0))"),
                    BID_STRT_TM = table.Column<DateTime>(type: "datetime", nullable: false),
                    BID_END_TM = table.Column<DateTime>(type: "datetime", nullable: false),
                    BID_STAT = table.Column<byte>(type: "tinyint", nullable: false),
                    BID_PROC_STAT = table.Column<byte>(type: "tinyint", nullable: false),
                    BID_NOTE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WARE_ID = table.Column<string>(type: "char(12)", unicode: false, fixedLength: true, maxLength: 12, nullable: false),
                    COMP_ID = table.Column<int>(type: "int", nullable: false),
                    ADD_BY = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ACT_BY = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PROC_BY = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CLOSE_BY = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ENTRY_TM = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    ACT_TIME = table.Column<DateTime>(type: "datetime", nullable: true),
                    PROC_TIME = table.Column<DateTime>(type: "datetime", nullable: true),
                    CLOSE_TIME = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMM_BID_MSTR", x => x.BID_ID);
                    table.ForeignKey(
                        name: "FK_COMM_BID_MSTR_COMM_COMP_INFO",
                        column: x => x.COMP_ID,
                        principalTable: "COMM_COMP_INFO",
                        principalColumn: "COMP_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_COMM_BID_MSTR_COMM_PROD_INFO",
                        column: x => x.PROD_ID,
                        principalTable: "COMM_PROD_INFO",
                        principalColumn: "PROD_ID");
                    table.ForeignKey(
                        name: "FK_COMM_BID_MSTR_COMM_WAREHS_INFO_WARE_ID",
                        column: x => x.WARE_ID,
                        principalTable: "COMM_WAREHS_INFO",
                        principalColumn: "WARE_ID");
                });

            migrationBuilder.CreateTable(
                name: "COMM_BID_CLNT_BIDDER",
                columns: table => new
                {
                    SYS_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BID_ID = table.Column<long>(type: "bigint", nullable: false),
                    PARTY_ID = table.Column<string>(type: "char(12)", unicode: false, fixedLength: true, maxLength: 12, nullable: false),
                    SMS_CONT_NO = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    SMS_REC_REF = table.Column<long>(type: "bigint", nullable: true),
                    SMS_REC_TM = table.Column<DateTime>(type: "datetime", nullable: true),
                    SMS_RAW_MSG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BID_QNTY = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValueSql: "((0))"),
                    BID_RATE = table.Column<decimal>(type: "money", nullable: true, defaultValueSql: "((0))"),
                    BID_ATTN_STAT = table.Column<byte>(type: "tinyint", nullable: true, defaultValueSql: "((0))"),
                    ALLOC_QNTY = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValueSql: "((0))"),
                    ALLOC_RATE = table.Column<decimal>(type: "money", nullable: true, defaultValueSql: "((0))"),
                    SMS_SEND_STAT = table.Column<byte>(type: "tinyint", nullable: true, defaultValueSql: "((0))"),
                    SMS_ALLOC_STAT = table.Column<byte>(type: "tinyint", nullable: true, defaultValueSql: "((0))"),
                    SMS_SEND_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SMS_RPLY_API = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMM_BID_CLNT_BIDDER", x => x.SYS_ID);
                    table.ForeignKey(
                        name: "FK_COMM_BID_CLNT_BIDDER_COMM_BID_MSTR",
                        column: x => x.BID_ID,
                        principalTable: "COMM_BID_MSTR",
                        principalColumn: "BID_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_COMM_BID_CLNT_BIDDER_COMM_PARTY_INFO",
                        column: x => x.PARTY_ID,
                        principalTable: "COMM_PARTY_INFO",
                        principalColumn: "PARTY_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_COMM_BID_CLNT_BIDDER_BID_ID",
                table: "COMM_BID_CLNT_BIDDER",
                column: "BID_ID");

            migrationBuilder.CreateIndex(
                name: "IX_COMM_BID_CLNT_BIDDER_PARTY_ID",
                table: "COMM_BID_CLNT_BIDDER",
                column: "PARTY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_COMM_BID_MSTR_COMP_ID",
                table: "COMM_BID_MSTR",
                column: "COMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_COMM_BID_MSTR_PROD_ID",
                table: "COMM_BID_MSTR",
                column: "PROD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_COMM_BID_MSTR_WARE_ID",
                table: "COMM_BID_MSTR",
                column: "WARE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_COMM_LOGIN_INFO",
                table: "COMM_LOGIN_INFO",
                columns: new[] { "USER_NM", "USER_PASS", "COMP_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_COMM_LOGIN_INFO_COMP_ID",
                table: "COMM_LOGIN_INFO",
                column: "COMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_COMM_NOTIFY_LIST",
                table: "COMM_NOTIFY_LIST",
                column: "IS_READ");

            migrationBuilder.CreateIndex(
                name: "IX_COMM_NOTIFY_LIST_NOT_BY",
                table: "COMM_NOTIFY_LIST",
                column: "NOT_BY");

            migrationBuilder.CreateIndex(
                name: "IX_COMM_PARTY_INFO",
                table: "COMM_PARTY_INFO",
                column: "CAT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_COMM_PARTY_INFO_COMP_ID",
                table: "COMM_PARTY_INFO",
                column: "COMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_COMM_PARTY_INFO_NMCM",
                table: "COMM_PARTY_INFO",
                columns: new[] { "PARTY_NAME", "COMP_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_COMM_PROD_INFO",
                table: "COMM_PROD_INFO",
                column: "CAT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_COMM_PROD_INFO_COMP_ID",
                table: "COMM_PROD_INFO",
                column: "COMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_COMM_PROD_INFO_NMCM",
                table: "COMM_PROD_INFO",
                columns: new[] { "PROD_NAME", "COMP_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_COMM_SMS_LOG",
                table: "COMM_SMS_LOG",
                column: "SMS_DT_TM");

            migrationBuilder.CreateIndex(
                name: "IX_COMM_WAREHS_INFO_COMP_ID",
                table: "COMM_WAREHS_INFO",
                column: "COMP_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "COMM_BID_CLNT_BIDDER");

            migrationBuilder.DropTable(
                name: "COMM_COUNTRY_INFO");

            migrationBuilder.DropTable(
                name: "COMM_NOTIFY_LIST");

            migrationBuilder.DropTable(
                name: "COMM_SMS_LOG");

            migrationBuilder.DropTable(
                name: "COMM_SMS_SETT");

            migrationBuilder.DropTable(
                name: "GetVoucherIdPKs");

            migrationBuilder.DropTable(
                name: "COMM_BID_MSTR");

            migrationBuilder.DropTable(
                name: "COMM_PARTY_INFO");

            migrationBuilder.DropTable(
                name: "COMM_LOGIN_INFO");

            migrationBuilder.DropTable(
                name: "COMM_PROD_INFO");

            migrationBuilder.DropTable(
                name: "COMM_WAREHS_INFO");

            migrationBuilder.DropTable(
                name: "COMM_PARTY_CAT");

            migrationBuilder.DropTable(
                name: "COMM_PROD_CAT");

            migrationBuilder.DropTable(
                name: "COMM_COMP_INFO");
        }
    }
}
