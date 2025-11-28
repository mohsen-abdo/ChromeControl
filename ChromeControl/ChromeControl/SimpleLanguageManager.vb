Public Class SimpleLanguageManager
    Public Shared IsArabic As Boolean = False

    ' ... (Form1 Code remains same) ...
    Public Shared Function GetForm1Text(key As String) As String
        If IsArabic Then
            Select Case key
                Case "title" : Return "التحكم في كروم v3.0 - 2025 Mohsen Abdo بواسطة"
                Case "select_profiles" : Return "اختر ملفات تعريف Chrome المراد فتحها:"
                Case "open_browsers" : Return "فتح المتصفحات المحددة"
                Case "pause" : Return "إيقاف مؤقت"
                Case "resume" : Return "استئناف"
                Case "stop" : Return "إيقاف"
                Case "close_chrome" : Return "إغلاق جميع نوافذ كروم"
                Case "uncheck_all" : Return "إلغاء تحديد الكل"
                Case "select_list" : Return "اختيار من القائمة"
                Case "clear_data" : Return "مسح بيانات المتصفح"
                Case "backup" : Return "نسخ احتياطي / استعادة"
                Case "enter_names" : Return "أدخل أسماء المتصفحات (كل اسم في سطر أو مفصولة بفاصلة):"
                Case "delay" : Return "التأخير بين فتح كل متصفح (بالثواني):"
                Case "url" : Return "رابط URL للفتح تلقائياً (اختياري)"
                Case "footer" : Return "Mohsen Abdo بواسطة arab-hasry.blogspot.com 2025 © التحكم في كروم"
                Case "lang_btn" : Return "English"
                Case "done" : Return "تم"
                Case "done_msg" : Return "تم فتح جميع المتصفحات بنجاح!"
                Case Else : Return key
            End Select
        Else
            Select Case key
                Case "title" : Return "Chrome Control v3.0 - 2025 By Mohsen Abdo"
                Case "select_profiles" : Return "Select Chrome Profiles To Open:"
                Case "open_browsers" : Return "Open Selected Browsers"
                Case "pause" : Return "Pause"
                Case "resume" : Return "Resume"
                Case "stop" : Return "Stop"
                Case "close_chrome" : Return "Close All Chrome Windows"
                Case "uncheck_all" : Return "Uncheck All"
                Case "select_list" : Return "Select From List"
                Case "clear_data" : Return "Clear Browser Data"
                Case "backup" : Return "Backup / Restore"
                Case "enter_names" : Return "Enter Browser Names (One Per Line Or Comma-Separated):"
                Case "delay" : Return "Delay Between Opening Each Browser (Seconds):"
                Case "url" : Return "URL To Open Automatically (Optional)"
                Case "footer" : Return "Chrome Control © 2025 arab-hasry.blogspot.com By Mohsen Abdoo"
                Case "lang_btn" : Return "العربية"
                Case "done" : Return "Complete"
                Case "done_msg" : Return "All browsers opened successfully!"
                Case Else : Return key
            End Select
        End If
    End Function

    ' ... (Form2 Code remains same) ...
    Public Shared Function GetForm2Text(key As String) As String
        If IsArabic Then
            Select Case key
                Case "title" : Return "التحكم في كروم - مسح بيانات المتصفح"
                Case "browsing_history" : Return "سجل التصفح"
                Case "download_history" : Return "سجل التنزيلات"
                Case "cookies" : Return "ملفات تعريف الارتباط وبيانات المواقع الأخرى"
                Case "cached" : Return "الصور والملفات المخزنة مؤقتاً"
                Case "passwords" : Return "كلمات المرور وبيانات تسجيل الدخول الأخرى"
                Case "autofill" : Return "بيانات الملء التلقائي للنماذج"
                Case "site_settings" : Return "إعدادات الموقع"
                Case "hosted_data" : Return "بيانات التطبيقات المستضافة"
                Case "select_clean" : Return "حدد ملف تعريف واحد أو أكثر لمسح بيانات التصفح"
                Case "check_all" : Return "تحديد الكل"
                Case "uncheck_all" : Return "إلغاء تحديد الكل"
                Case "clean_btn" : Return "تنظيف المتصفحات المحددة"
                Case "selected_count" : Return "عدد المتصفحات المحددة:"
                Case "confirm_clean" : Return "سيتم إغلاق جميع نوافذ كروم وحذف البيانات المحددة. هل أنت متأكد؟"
                Case "wait_clean" : Return "عملية تنظيف أخرى قيد التشغيل."
                Case Else : Return key
            End Select
        Else
            Select Case key
                Case "title" : Return "Chrome Control - Clear Browser Data"
                Case "browsing_history" : Return "Browsing History"
                Case "download_history" : Return "Download History"
                Case "cookies" : Return "Cookies And Other Site Data"
                Case "cached" : Return "Cached Images And Files"
                Case "passwords" : Return "Passwords And Other Sign-In Data"
                Case "autofill" : Return "Autofill Form Data"
                Case "site_settings" : Return "Site Settings"
                Case "hosted_data" : Return "Hosted App Data"
                Case "select_clean" : Return "Select One Or More Chrome Profiles To Clean Browsing Data"
                Case "check_all" : Return "Check All"
                Case "uncheck_all" : Return "Uncheck All"
                Case "clean_btn" : Return "Clean Selected Browsers"
                Case "selected_count" : Return "Selected Browsers Count:"
                Case "confirm_clean" : Return "All Chrome Windows Will Be Closed And Selected Data Will Be Deleted. Continue?"
                Case "wait_clean" : Return "Another Cleaning Operation Is Running."
                Case Else : Return key
            End Select
        End If
    End Function

    ' Form3 - Progress
    Public Shared Function GetForm3Text(key As String) As String
        If IsArabic Then
            Select Case key
                Case "title" : Return "التحكم في كروم - تقدم العمليات"
                Case "cancel" : Return "إلغاء العملية"
                Case "close" : Return "إغلاق"
                Case "percentage" : Return "النسبة المئوية"
                Case "confirm_cancel" : Return "هل أنت متأكد أنك تريد إلغاء العملية الحالية؟"
                Case "cancelled_user" : Return "تم إلغاء العملية بناءً على طلبك."
                Case Else : Return key
            End Select
        Else
            Select Case key
                Case "title" : Return "Chrome Control - Operations Progress"
                Case "cancel" : Return "Cancel Operation"
                Case "close" : Return "Close"
                Case "percentage" : Return "Percentage"
                Case "confirm_cancel" : Return "Are You Sure You Want To Cancel The Current Operation?"
                Case "cancelled_user" : Return "Operation Cancelled By Your Request."
                Case Else : Return key
            End Select
        End If
    End Function

    ' Form4 - Backup (Updated Texts)
    Public Shared Function GetForm4Text(key As String) As String
        If IsArabic Then
            Select Case key
                Case "title" : Return "التحكم في كروم - أداة النسخ الاحتياطي"
                Case "select_backup" : Return "حدد الملفات الشخصية التي تريد نسخها احتياطياً"
                Case "check_all" : Return "تحديد الكل"
                Case "uncheck_all" : Return "إلغاء تحديد الكل"
                Case "start_backup" : Return "بدء النسخ الاحتياطي"
                Case "selected_count" : Return "عدد المتصفحات المحددة:"
                Case "backup_profiles" : Return "نسخ الملفات الشخصية"
                Case "backup_passwords" : Return "تصدير كلمات المرور (CSV)"
                Case "backup_registry" : Return "نسخ مفاتيح التسجيل (Registry)"
                Case "msg_no_profile" : Return "الرجاء تحديد ملف تعريف واحد على الأقل."
                Case "msg_no_option" : Return "الرجاء اختيار عملية واحدة على الأقل."
                Case "msg_busy" : Return "يوجد عملية جارية، الرجاء الانتظار."
                Case "msg_confirm" : Return "سيتم إغلاق كروم وبدء العملية. هل تود المتابعة؟"

                ' تحديث النصوص هنا لتكون أكثر تفصيلاً
                Case "cleaning_old" : Return "جاري تهيئة المجلدات..."
                Case "creating_folders" : Return "جاري إنشاء مجلدات النسخ..."
                Case "copying_reg" : Return "جاري الآن نسخ مفاتيح الريجستري..."
                Case "copying_profile" : Return "جاري نسخ ملفات البروفايل: "
                Case "exporting_pass" : Return "جاري تصدير كلمات مرور البروفايل: "

                Case "options_group" : Return "خيارات النسخ الاحتياطي"
                Case "retry_msg" : Return "لم يتم تحديد ملف كلمات المرور! هل تريد المحاولة مرة أخرى؟"
                Case "retry_title" : Return "لم يتم الحفظ"
                Case "pass_title" : Return "تصدير كلمات المرور"
                Case "pass_msg_1" : Return "1. في الصفحة المفتوحة، اضغط 'Download file' لتحميل الملف."
                Case "pass_msg_2" : Return "2. أدخل كلمة مرور الويندوز (أو PIN) إذا طُلبت منك."
                Case "pass_msg_3" : Return "3. احفظ الملف في مكان يسهل الوصول إليه."
                Case "pass_msg_4" : Return "4. في حال فشل الرابط، انسخه من الأسفل وافتحه يدوياً."
                Case "pass_msg_5" : Return "5. بعد الحفظ، اضغط 'موافق' (OK) هنا."
                Case Else : Return key
            End Select
        Else
            Select Case key
                Case "title" : Return "Chrome Control - Backup Tool"
                Case "select_backup" : Return "Select The Profiles You Want To Backup"
                Case "check_all" : Return "Check All"
                Case "uncheck_all" : Return "Uncheck All"
                Case "start_backup" : Return "Start Backup"
                Case "selected_count" : Return "Selected Browsers Count:"
                Case "backup_profiles" : Return "Backup Profile Files"
                Case "backup_passwords" : Return "Backup Passwords (CSV Export)"
                Case "backup_registry" : Return "Backup Registry Keys"
                Case "msg_no_profile" : Return "Please Select At Least One Profile."
                Case "msg_no_option" : Return "Please Select At Least One Option."
                Case "msg_busy" : Return "Operation In Progress, Please Wait."
                Case "msg_confirm" : Return "Chrome Will Close And Operation Will Start. Continue?"

                Case "cleaning_old" : Return "Initializing Folders..."
                Case "creating_folders" : Return "Creating Folders..."
                Case "copying_reg" : Return "Currently backing up Registry Keys..."
                Case "copying_profile" : Return "Copying files for Profile: "
                Case "exporting_pass" : Return "Exporting passwords for Profile: "

                Case "options_group" : Return "Backup Options"
                Case "retry_msg" : Return "Password file was not selected! Do you want to try again?"
                Case "retry_title" : Return "Not Saved"
                Case "pass_title" : Return "Export Passwords"
                Case "pass_msg_1" : Return "1. In the opened page, click 'Download file'."
                Case "pass_msg_2" : Return "2. Enter your Windows Password (or PIN) if asked."
                Case "pass_msg_3" : Return "3. Save the file in an accessible location."
                Case "pass_msg_4" : Return "4. If link failed, copy link below and open manually."
                Case "pass_msg_5" : Return "5. After saving, Click 'OK' on this message."
                Case Else : Return key
            End Select
        End If
    End Function
End Class