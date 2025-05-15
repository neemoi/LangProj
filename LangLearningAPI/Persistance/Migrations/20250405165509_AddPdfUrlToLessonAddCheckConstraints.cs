using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistance.Migrations
{
    public partial class AddPdfUrlToLessonAddCheckConstraints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PdfUrl",
                table: "Lessons",
                type: "longtext",
                nullable: true);

            migrationBuilder.Sql(@"
CREATE PROCEDURE DropConstraintIfExists(IN tableName VARCHAR(64), IN constraintName VARCHAR(64))
BEGIN
    SET @dbname = DATABASE();
    SET @tablename = tableName;
    SET @constraintname = constraintName;
    SET @preparedStatement = (SELECT IF(
        (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
         WHERE CONSTRAINT_SCHEMA = @dbname 
         AND TABLE_NAME = @tablename 
         AND CONSTRAINT_NAME = @constraintname) > 0,
        CONCAT('ALTER TABLE ', @tablename, ' DROP CONSTRAINT ', @constraintname),
        'SELECT 1'
    ));
    PREPARE stmt FROM @preparedStatement;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
END;
");

            migrationBuilder.Sql(@"
CREATE PROCEDURE ApplyConstraintsIfTablesExist()
BEGIN
    DECLARE quizzes_exists INT;
    DECLARE lessonwords_exists INT;
    DECLARE quizquestions_exists INT;
    DECLARE userwordprogress_exists INT;

    SELECT COUNT(*) INTO quizzes_exists 
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'quizzes';

    SELECT COUNT(*) INTO lessonwords_exists 
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'lessonwords';

    SELECT COUNT(*) INTO quizquestions_exists 
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'quizquestions';

    SELECT COUNT(*) INTO userwordprogress_exists 
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'userwordprogress';

    IF quizzes_exists > 0 THEN
        CALL DropConstraintIfExists('quizzes', 'CK_Quiz_Type');
        UPDATE quizzes SET Type = 'Nouns'
        WHERE Type IS NULL OR Type NOT IN ('Nouns', 'Grammar');
        ALTER TABLE quizzes
        ADD CONSTRAINT CK_Quiz_Type
        CHECK (Type IN ('Nouns', 'Grammar'));
    END IF;

    IF lessonwords_exists > 0 THEN
        CALL DropConstraintIfExists('lessonwords', 'CK_LessonWord_Type');
        UPDATE lessonwords SET Type = 'Keyword'
        WHERE Type IS NULL OR Type NOT IN ('Keyword', 'Additional');
        ALTER TABLE lessonwords
        ADD CONSTRAINT CK_LessonWord_Type
        CHECK (Type IN ('Keyword', 'Additional'));
    END IF;

    IF quizquestions_exists > 0 THEN
        CALL DropConstraintIfExists('quizquestions', 'CK_QuizQuestion_QuestionType');
        UPDATE quizquestions SET QuestionType = 'ImageChoice'
        WHERE QuestionType IS NULL OR QuestionType NOT IN (
            'ImageChoice', 'AudioChoice', 'ImageAudioChoice', 
            'Spelling', 'GrammarSelection', 'Pronunciation', 'AdvancedSurvey'
        );
        ALTER TABLE quizquestions
        ADD CONSTRAINT CK_QuizQuestion_QuestionType
        CHECK (QuestionType IN (
            'ImageChoice', 'AudioChoice', 'ImageAudioChoice', 
            'Spelling', 'GrammarSelection', 'Pronunciation', 'AdvancedSurvey'
        ));
    END IF;

    IF userwordprogress_exists > 0 THEN
        CALL DropConstraintIfExists('userwordprogress', 'CK_UserWordProgress_QuestionType');
        UPDATE userwordprogress SET QuestionType = 'ImageChoice'
        WHERE QuestionType IS NULL OR QuestionType NOT IN (
            'ImageChoice', 'AudioChoice', 'ImageAudioChoice', 
            'Spelling', 'GrammarSelection', 'Pronunciation', 'AdvancedSurvey'
        );
        ALTER TABLE userwordprogress
        ADD CONSTRAINT CK_UserWordProgress_QuestionType
        CHECK (QuestionType IN (
            'ImageChoice', 'AudioChoice', 'ImageAudioChoice', 
            'Spelling', 'GrammarSelection', 'Pronunciation', 'AdvancedSurvey'
        ));
    END IF;
END;
");

            migrationBuilder.Sql("CALL ApplyConstraintsIfTablesExist();");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS ApplyConstraintsIfTablesExist;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS DropConstraintIfExists;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfUrl",
                table: "Lessons");

            DropConstraintIfExists(migrationBuilder, "quizzes", "CK_Quiz_Type");
            DropConstraintIfExists(migrationBuilder, "lessonwords", "CK_LessonWord_Type");
            DropConstraintIfExists(migrationBuilder, "quizquestions", "CK_QuizQuestion_QuestionType");
            DropConstraintIfExists(migrationBuilder, "userwordprogress", "CK_UserWordProgress_QuestionType");
        }

        private void DropConstraintIfExists(MigrationBuilder migrationBuilder, string tableName, string constraintName)
        {
            migrationBuilder.Sql($@"
SET @dbname = DATABASE();
SET @tablename = '{tableName}';
SET @constraintname = '{constraintName}';
SET @preparedStatement = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
     WHERE CONSTRAINT_SCHEMA = @dbname 
     AND TABLE_NAME = @tablename 
     AND CONSTRAINT_NAME = @constraintname) > 0,
    CONCAT('ALTER TABLE ', @tablename, ' DROP CONSTRAINT ', @constraintname),
    'SELECT 1'
));
PREPARE stmt FROM @preparedStatement;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;");
        }
    }
}
