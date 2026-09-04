import { Injectable, inject, effect, NgZone } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { StateService } from './state.service';
import enDict from '../../../assets/i18n/en.json';
import arDict from '../../../assets/i18n/ar.json';

@Injectable({
  providedIn: 'root'
})
export class TranslationService {
  private state = inject(StateService);
  private ngZone = inject(NgZone);
  private router = inject(Router, { optional: true });

  private translations: Record<string, Record<string, string>> = {
    en: enDict as Record<string, string>,
    ar: arDict as Record<string, string>
  };

  private englishToArMap = new Map<string, string>();
  private arabicToEnMap = new Map<string, string>();
  private originalTextMap = new WeakMap<Node, string>();
  private observer?: MutationObserver;
  private debounceTimer: any = null;
  private isTranslating = false;
  private isInitialized = false;

  constructor() {
    this.buildLookupMaps();
    this.setupAutoDomTranslator();
  }

  /** Explicit initializer to guarantee eager startup from provideAppInitializer */
  init(): void {
    if (this.isInitialized) return;
    this.isInitialized = true;
    if (typeof window !== 'undefined') {
      setTimeout(() => {
        this.runFullTranslation();
      }, 50);
    }
  }

  private buildLookupMaps(): void {
    this.englishToArMap.clear();
    this.arabicToEnMap.clear();

    // 1. Direct pairs in arDict (English Key -> Arabic Value)
    for (const [k, v] of Object.entries(arDict)) {
      if (typeof v === 'string' && v.trim()) {
        const enClean = k.trim();
        const arClean = v.trim();
        this.englishToArMap.set(enClean.toLowerCase(), arClean);
        this.arabicToEnMap.set(arClean.toLowerCase(), enClean);
      }
    }

    // 2. Additional pairs in enDict
    for (const [k, enVal] of Object.entries(enDict)) {
      const arVal = (arDict as Record<string, string>)[k];
      if (typeof enVal === 'string' && typeof arVal === 'string' && enVal.trim() && arVal.trim()) {
        const enClean = enVal.trim();
        const arClean = arVal.trim();
        this.englishToArMap.set(enClean.toLowerCase(), arClean);
        this.arabicToEnMap.set(arClean.toLowerCase(), enClean);
      }
    }
  }

  /**
   * Retrieves translation for the given key based on current language.
   * Fully bidirectional: translates English->Arabic in 'ar' mode, and Arabic->English in 'en' mode.
   */
  get(key: string, defaultVal?: string): string {
    if (!key) return '';
    const currentLang = this.state.lang() || 'en';
    const trimmed = key.trim();

    if (currentLang === 'ar') {
      // 1. Direct exact match in arDict
      const direct = (arDict as Record<string, string>)[trimmed] || (arDict as Record<string, string>)[key];
      if (direct !== undefined) return direct;

      // 2. Normalized lowercase lookup
      const normalized = this.englishToArMap.get(trimmed.toLowerCase());
      if (normalized) return normalized;

      // 3. Count suffix match: e.g. "All Leads (5)" -> "جميع العملاء المحتملين (5)"
      const countMatch = trimmed.match(/^(.*?)\s*(\(\d+\)|\[\d+\]|\d+)$/);
      if (countMatch) {
        const base = countMatch[1].trim();
        const suffix = countMatch[2];
        const baseTrans = this.englishToArMap.get(base.toLowerCase());
        if (baseTrans) {
          return `${baseTrans} ${suffix}`;
        }
      }

      // 4. Leading count + label: e.g. "5 records", "0 rows", "3 users".
      const leadingCountMatch = trimmed.match(/^(\d[\d,\.]*?)\s+(.+)$/);
      if (leadingCountMatch) {
        const count = leadingCountMatch[1];
        const label = leadingCountMatch[2].trim();
        const labelTrans = this.englishToArMap.get(label.toLowerCase());
        if (labelTrans) {
          return `${count} ${labelTrans}`;
        }
      }

      // 5. Prefix + dynamic value: e.g. "Last run: 2026-09-04".
      const prefixedValueMatch = trimmed.match(/^(Last run:|Generated at)\s+(.+)$/i);
      if (prefixedValueMatch) {
        const prefix = prefixedValueMatch[1].trim();
        const rest = prefixedValueMatch[2].trim();
        const prefixTrans = this.englishToArMap.get(prefix.toLowerCase());
        if (prefixTrans) {
          return `${prefixTrans} ${rest}`;
        }
      }

      // 6. Punctuation-stripped lookup (e.g., "Feature.", "Total:", "Debit ($)")
      const stripped = trimmed.replace(/^[\s\/\-\:\#\$\(\)]+|[\s\.\:\,\-\!\?\(\)]+$/g, '');
      if (stripped && stripped !== trimmed) {
        const strippedNormalized = this.englishToArMap.get(stripped.toLowerCase());
        if (strippedNormalized) {
          let result = strippedNormalized;
          if (trimmed.startsWith('/ ')) result = '/ ' + result;
          if (trimmed.endsWith('.')) result = result + '.';
          if (trimmed.endsWith(':')) result = result + ':';
          return result;
        }
      }

      // 5. Dynamic Toast / Notification Pattern Matching
      const dynamicAr = this.matchDynamicToast(trimmed, true);
      if (dynamicAr) return dynamicAr;

      return defaultVal !== undefined ? defaultVal : key;
    }

    // English mode: If key is Arabic, look up English; otherwise return directEn or key
    if (/[\u0600-\u06FF]/.test(trimmed)) {
      const dynamicEn = this.matchDynamicToast(trimmed, false);
      if (dynamicEn) return dynamicEn;
      const en = this.getEnglish(trimmed);
      if (en) return en;
    }

    const directEn = (enDict as Record<string, string>)[trimmed] || (enDict as Record<string, string>)[key];
    if (directEn !== undefined) return directEn;

    return defaultVal !== undefined ? defaultVal : key;
  }

  matchDynamicToast(text: string, toArabic: boolean): string | null {
    if (!text) return null;
    const trimmed = text.trim();

    if (toArabic) {
      // "Subscription upgraded to Free Plan!"
      const subMatch = trimmed.match(/^Subscription upgraded to (.*?)!$/i);
      if (subMatch) {
        const plan = subMatch[1].trim();
        const planAr = (arDict as Record<string, string>)[plan] || this.englishToArMap.get(plan.toLowerCase()) || plan;
        return `تمت ترقية الاشتراك إلى ${planAr}!`;
      }

      // "Feature limits updated for Free Plan"
      const featMatch = trimmed.match(/^Feature limits updated for (.*?)$/i);
      if (featMatch) {
        const plan = featMatch[1].trim();
        const planAr = (arDict as Record<string, string>)[plan] || this.englishToArMap.get(plan.toLowerCase()) || plan;
        return `تم تحديث حدود الميزات لـ ${planAr}`;
      }

      // "File \"abc\" was imported successfully."
      const fileMatch = trimmed.match(/^File\s+["']?(.*?)["']?\s+was imported successfully\.?$/i);
      if (fileMatch) {
        return `تم استيراد الملف "${fileMatch[1]}" بنجاح.`;
      }

      // "Check-in logged for Ahmed at 09:30"
      const checkinMatch = trimmed.match(/^Check-in logged for (.*?) at (.*?)$/i);
      if (checkinMatch) {
        return `تم تسجيل الحضور لـ ${checkinMatch[1]} في ${checkinMatch[2]}`;
      }

      // "Group \"xyz\" created."
      const groupMatch = trimmed.match(/^Group\s+["']?(.*?)["']?\s+created\.?$/i);
      if (groupMatch) {
        return `تم إنشاء المجموعة "${groupMatch[1]}".`;
      }

      // "5 member(s) added."
      const memberMatch = trimmed.match(/^(\d+)\s+member\(s\)\s+added\.?$/i);
      if (memberMatch) {
        return `تمت إضافة ${memberMatch[1]} عضو/أعضاء.`;
      }

      // "Candidate John moved to Interview."
      const moveMatch = trimmed.match(/^(.*?)\s+moved to (.*?)\.?$/i);
      if (moveMatch) {
        const target = moveMatch[1].trim();
        const stage = moveMatch[2].trim();
        const stageAr = (arDict as Record<string, string>)[stage] || this.englishToArMap.get(stage.toLowerCase()) || stage;
        return `تم نقل ${target} إلى ${stageAr}.`;
      }

      // "Product SKU-101 added to catalog."
      const prodMatch = trimmed.match(/^Product\s+(.*?)\s+added to catalog\.?$/i);
      if (prodMatch) {
        return `تمت إضافة المنتج ${prodMatch[1]} إلى الدليل.`;
      }

      // "Stock adjusted for Widget."
      const stockMatch = trimmed.match(/^Stock adjusted for (.*?)\.?$/i);
      if (stockMatch) {
        return `تم تعديل المخزون لـ ${stockMatch[1]}.`;
      }

      // "Purchase order PO-001 approved."
      const poMatch = trimmed.match(/^Purchase order\s+(.*?)\s+approved\.?$/i);
      if (poMatch) {
        return `تمت الموافقة على أمر الشراء ${poMatch[1]}.`;
      }

      // "Generated \"Report\" (100 rows)."
      const genMatch = trimmed.match(/^Generated\s+["']?(.*?)["']?\s+\((\d+)\s+rows\)\.?$/i);
      if (genMatch) {
        const title = (arDict as Record<string, string>)[genMatch[1]] || genMatch[1];
        return `تم توليد "${title}" (${genMatch[2]} سجل).`;
      }

      // "Exported \"Report\" as CSV."
      const expMatch = trimmed.match(/^Exported\s+["']?(.*?)["']?\s+as CSV\.?$/i);
      if (expMatch) {
        const title = (arDict as Record<string, string>)[expMatch[1]] || expMatch[1];
        return `تم تصدير "${title}" كملف CSV.`;
      }

      // "Payroll for August 2026 processed & approved successfully."
      const payMatch = trimmed.match(/^Payroll for (.*?)\s+processed\s+&\s+approved successfully\.?$/i);
      if (payMatch) {
        return `تمت معالجة واعتماد مسير رواتب ${payMatch[1]} بنجاح.`;
      }

      // "Payslip PDF for John downloaded successfully."
      const payslipMatch = trimmed.match(/^Payslip PDF for (.*?)\s+downloaded successfully\.?$/i);
      if (payslipMatch) {
        return `تم تحميل قسيمة الراتب PDF لـ ${payslipMatch[1]} بنجاح.`;
      }

      // "Double-entry mismatch! Debit ($X) must equal Credit ($Y)."
      const debitMatch = trimmed.match(/Double-entry mismatch! Debit \((.*?)\) must equal Credit \((.*?)\)/i);
      if (debitMatch) {
        return `عدم تطابق في القيد المزدوج! المدين (${debitMatch[1]}) يجب أن يساوي الدائن (${debitMatch[2]}).`;
      }

      // "New message from Ahmed in #general"
      const msgMatch = trimmed.match(/^New message from (.*?) in #(.*?)$/i);
      if (msgMatch) {
        return `رسالة جديدة من ${msgMatch[1]} في #${msgMatch[2]}`;
      }

      // "Are you sure you want to delete Ahmed Hamdi? This action cannot be undone."
      // Built from a template literal, so it never matches a static dictionary key.
      const deleteMatch = trimmed.match(
        /^Are you sure you want to delete\s+(.+?)\s*\?\s*This action cannot be undone\.?$/i
      );
      if (deleteMatch) {
        return `هل أنت متأكد من حذف ${deleteMatch[1]}؟ لا يمكن التراجع عن هذا الإجراء.`;
      }

      // "Could not generate \"X\"." / "Could not export \"X\"."
      const genFailMatch = trimmed.match(/^Could not generate\s+["']?(.*?)["']?\.?$/i);
      if (genFailMatch) {
        const title = (arDict as Record<string, string>)[genFailMatch[1]] || genFailMatch[1];
        return `تعذر إنشاء "${title}".`;
      }
      const expFailMatch = trimmed.match(/^Could not export\s+["']?(.*?)["']?\.?$/i);
      if (expFailMatch) {
        const title = (arDict as Record<string, string>)[expFailMatch[1]] || expFailMatch[1];
        return `تعذر تصدير "${title}".`;
      }

      // "Access policy for \"X\" saved." / "Role \"X\" created."
      const policyMatch = trimmed.match(/^Access policy for\s+["']?(.*?)["']?\s+saved\.?$/i);
      if (policyMatch) {
        const role = (arDict as Record<string, string>)[policyMatch[1]] || policyMatch[1];
        return `تم حفظ سياسة الوصول لـ "${role}".`;
      }
      const roleMatch = trimmed.match(/^Role\s+["']?(.*?)["']?\s+created\.?$/i);
      if (roleMatch) {
        const role = (arDict as Record<string, string>)[roleMatch[1]] || roleMatch[1];
        return `تم إنشاء الدور "${role}".`;
      }

      // Document manager delete confirm (DialogService message)
      const delFolderMatch = trimmed.match(/^Delete folder\s+["']?(.*?)["']?\?$/i);
      if (delFolderMatch) {
        return `حذف مجلد "${delFolderMatch[1]}"؟`;
      }
      const delDocMatch = trimmed.match(/^Delete\s+["']?(.*?)["']?\?$/i);
      if (delDocMatch) {
        return `حذف "${delDocMatch[1]}"؟`;
      }

      // "\"X\" was processed successfully." (file-import dialog, dynamic filename)
      const processedMatch = trimmed.match(/^["']?(.*?)["']?\s+was processed successfully\.?$/i);
      if (processedMatch) {
        return `تمت معالجة "${processedMatch[1]}" بنجاح.`;
      }

      // "No matching items found for \"X\"" (global search modal, dynamic query)
      const noMatchMatch = trimmed.match(/^No matching items found for\s+["']?(.*?)["']?$/i);
      if (noMatchMatch) {
        return `لم يتم العثور على عناصر مطابقة لـ "${noMatchMatch[1]}".`;
      }
    } else {
      // Reverse: Arabic -> English
      const subMatch = trimmed.match(/^تمت ترقية الاشتراك إلى (.*?)!$/);
      if (subMatch) {
        const plan = subMatch[1].trim();
        const planEn = this.arabicToEnMap.get(plan.toLowerCase()) || plan;
        return `Subscription upgraded to ${planEn}!`;
      }

      const featMatch = trimmed.match(/^تم تحديث حدود الميزات لـ (.*?)$/);
      if (featMatch) {
        const plan = featMatch[1].trim();
        const planEn = this.arabicToEnMap.get(plan.toLowerCase()) || plan;
        return `Feature limits updated for ${planEn}`;
      }

      const fileMatch = trimmed.match(/^تم استيراد الملف ["']?(.*?)["']? بنجاح\.?$/);
      if (fileMatch) {
        return `File "${fileMatch[1]}" was imported successfully.`;
      }

      // Reverse of the delete-confirmation pattern above.
      const deleteAr = trimmed.match(
        /^هل أنت متأكد من حذف\s+(.+؟)\s*لا يمكن التراجع عن هذا الإجراء\.?$/
      );
      if (deleteAr) {
        return `Are you sure you want to delete ${deleteAr[1]}? This action cannot be undone.`;
      }

      // Reverse of generate/export failure
      const genFailAr = trimmed.match(/^تعذر إنشاء ["']?(.*?)["']?\.?$/);
      if (genFailAr) {
        const titleEn = this.arabicToEnMap.get(genFailAr[1].toLowerCase()) || genFailAr[1];
        return `Could not generate "${titleEn}".`;
      }
      const expFailAr = trimmed.match(/^تعذر تصدير ["']?(.*?)["']?\.?$/);
      if (expFailAr) {
        const titleEn = this.arabicToEnMap.get(expFailAr[1].toLowerCase()) || expFailAr[1];
        return `Could not export "${titleEn}".`;
      }

      // Reverse of access policy / role created
      const policyAr = trimmed.match(/^تم حفظ سياسة الوصول لـ ["']?(.*?)["']?\.?$/);
      if (policyAr) {
        const roleEn = this.arabicToEnMap.get(policyAr[1].toLowerCase()) || policyAr[1];
        return `Access policy for "${roleEn}" saved.`;
      }
      const roleAr = trimmed.match(/^تم إنشاء الدور ["']?(.*?)["']?\.?$/);
      if (roleAr) {
        const roleEn = this.arabicToEnMap.get(roleAr[1].toLowerCase()) || roleAr[1];
        return `Role "${roleEn}" created.`;
      }

      // Reverse of document delete confirm
      const delFolderAr = trimmed.match(/^حذف مجلد ["']?(.*?)["']?؟$/);
      if (delFolderAr) {
        return `Delete folder "${delFolderAr[1]}"?`;
      }
      const delDocAr = trimmed.match(/^حذف ["']?(.*?)["']?؟$/);
      if (delDocAr) {
        return `Delete "${delDocAr[1]}"?`;
      }

      // Reverse of "X" was processed successfully
      const processedAr = trimmed.match(/^تمت معالجة ["']?(.*?)["']? بنجاح\.?$/);
      if (processedAr) {
        return `"${processedAr[1]}" was processed successfully.`;
      }

      // Reverse of no matching items
      const noMatchAr = trimmed.match(/^لم يتم العثور على عناصر مطابقة لـ ["']?(.*?)["']?\.?$/);
      if (noMatchAr) {
        return `No matching items found for "${noMatchAr[1]}"`;
      }
    }

    return null;
  }

  /** Reverse lookup: translates Arabic text to English */
  getEnglish(arabicText: string): string | null {
    if (!arabicText) return null;
    const trimmed = arabicText.trim();

    // 1. Direct exact normalized lookup
    const direct = this.arabicToEnMap.get(trimmed.toLowerCase());
    if (direct) return direct;

    // 2. Count suffix match: e.g. "جميع العملاء المحتملين (5)" -> "All Leads (5)"
    const countMatch = trimmed.match(/^(.*?)\s*(\(\d+\)|\[\d+\]|\d+)$/);
    if (countMatch) {
      const base = countMatch[1].trim();
      const suffix = countMatch[2];
      const baseEn = this.arabicToEnMap.get(base.toLowerCase());
      if (baseEn) {
        return `${baseEn} ${suffix}`;
      }
    }

    // 3. Leading count + label: e.g. "5 سجل" -> "5 records".
    const leadingCountMatch = trimmed.match(/^(\d[\d,\.]*?)\s+(.+)$/);
    if (leadingCountMatch) {
      const count = leadingCountMatch[1];
      const label = leadingCountMatch[2].trim();
      const labelEn = this.arabicToEnMap.get(label.toLowerCase());
      if (labelEn) {
        return `${count} ${labelEn}`;
      }
    }

    // 4. Prefix + dynamic value: e.g. "آخر تشغيل: 2026-09-04".
    const prefixedValueMatch = trimmed.match(/^(آخر تشغيل:|تم الإنشاء في)\s+(.+)$/);
    if (prefixedValueMatch) {
      const prefix = prefixedValueMatch[1].trim();
      const rest = prefixedValueMatch[2].trim();
      const prefixEn = this.arabicToEnMap.get(prefix.toLowerCase());
      if (prefixEn) {
        return `${prefixEn} ${rest}`;
      }
    }

    // 5. Punctuation-stripped lookup
    const stripped = trimmed.replace(/^[\s\/\-\:\#\$\(\)]+|[\s\.\:\,\-\!\?\(\)]+$/g, '');
    if (stripped && stripped !== trimmed) {
      const strippedEn = this.arabicToEnMap.get(stripped.toLowerCase());
      if (strippedEn) {
        let result = strippedEn;
        if (trimmed.startsWith('/ ')) result = '/ ' + result;
        if (trimmed.endsWith('.')) result = result + '.';
        if (trimmed.endsWith(':')) result = result + ':';
        return result;
      }
    }

    return null;
  }

  instant(key: string, defaultVal?: string): string {
    return this.get(key, defaultVal);
  }

  /**
   * Translates an HTML container and all its child elements using a full DOM TreeWalker.
   * In 'ar' mode: translates English -> Arabic.
   * In 'en' mode: translates Arabic -> English.
   */
  translateElement(root: Element | Document): void {
    if (typeof window === 'undefined' || !root) return;
    if (this.isTranslating) return;

    this.isTranslating = true;
    const isAr = this.state.lang() === 'ar';

    try {
      // 1. Walk every text node in the subtree
      const doc = (root.ownerDocument || (root.nodeType === Node.DOCUMENT_NODE ? root : document)) as Document;
      const walker = doc.createTreeWalker(
        root,
        NodeFilter.SHOW_TEXT,
        {
          acceptNode: (node: Node) => {
            const parent = node.parentElement;
            if (!parent) return NodeFilter.FILTER_REJECT;
            const tag = parent.tagName.toLowerCase();
            if (tag === 'script' || tag === 'style' || tag === 'noscript' || tag === 'code' || tag === 'pre') {
              return NodeFilter.FILTER_REJECT;
            }
            if (parent.closest('.notranslate, [data-no-translate], [translate="no"], .lang-switch-btn, #lang-switch-btn')) {
              return NodeFilter.FILTER_REJECT;
            }
            return NodeFilter.FILTER_ACCEPT;
          }
        }
      );

      const textNodes: Node[] = [];
      let currentNode: Node | null = walker.nextNode();
      while (currentNode) {
        textNodes.push(currentNode);
        currentNode = walker.nextNode();
      }

      for (const node of textNodes) {
        this.processTextNode(node, isAr);
      }

      // 2. Translate Input and Textarea Placeholders bidirectionally
      const inputs = (root as Element).querySelectorAll
        ? (root as Element).querySelectorAll('input[placeholder], textarea[placeholder]')
        : [];
      inputs.forEach(inputEl => {
        const input = inputEl as HTMLInputElement | HTMLTextAreaElement;
        const currentPh = input.getAttribute('placeholder');
        if (!currentPh) return;

        if (isAr) {
          const original = input.dataset['origPlaceholder'] || currentPh;
          if (!input.dataset['origPlaceholder'] && !/[\u0600-\u06FF]/.test(currentPh)) {
            input.dataset['origPlaceholder'] = currentPh;
          }
          const translated = this.get(original);
          if (translated && translated !== original) {
            input.setAttribute('placeholder', translated);
          }
        } else {
          // Revert to English
          const orig = input.dataset['origPlaceholder'];
          if (orig && !/[\u0600-\u06FF]/.test(orig)) {
            input.setAttribute('placeholder', orig);
          } else {
            const enPh = this.getEnglish(currentPh);
            if (enPh) {
              input.setAttribute('placeholder', enPh);
            }
          }
        }
      });
    } finally {
      this.isTranslating = false;
    }
  }

  private cleanIsoDates(text: string): string {
    if (!text || text.indexOf('T') === -1) return text;

    // 1. Midnight dates: 2026-09-04T00:00:00(.000)?(Z)? -> 2026-09-04
    let cleaned = text.replace(
      /\b(\d{4}-\d{2}-\d{2})T00:00:00(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})?\b/gi,
      '$1'
    );

    // 2. Dates with time: 2026-09-04T15:30:00(.000)?(Z)? -> 2026-09-04 15:30
    cleaned = cleaned.replace(
      /\b(\d{4}-\d{2}-\d{2})T(\d{2}:\d{2})(?::\d{2})?(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})?\b/gi,
      '$1 $2'
    );

    return cleaned;
  }

  private processTextNode(node: Node, isAr: boolean): void {
    const parent = node.parentElement;
    if (parent && parent.closest('.notranslate, [data-no-translate], [translate="no"], .lang-switch-btn, #lang-switch-btn')) {
      return;
    }

    let rawContent = node.textContent;
    if (!rawContent) return;

    // Clean any unformatted ISO dates like 2026-09-04T00:00:00 across all languages
    if (rawContent.indexOf('T') !== -1 && /\b\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/.test(rawContent)) {
      const formatted = this.cleanIsoDates(rawContent);
      if (formatted !== rawContent) {
        node.textContent = formatted;
        rawContent = formatted;
      }
    }

    const trimmed = rawContent.trim();
    if (trimmed.length < 2) return;

    // Ignore pure numbers, currencies, dates, single symbols (e.g. "$299", "100%", "2026-09-04")
    if (/^[\d\s\.,\-\/:$%#+*()@!؟]+$/.test(trimmed)) return;

    if (isAr) {
      // 1. If text is already Arabic, nothing to do
      if (/[\u0600-\u06FF]/.test(trimmed)) {
        return;
      }

      // Store clean English original text
      if (!this.originalTextMap.has(node)) {
        this.originalTextMap.set(node, rawContent);
      }
      const original = this.originalTextMap.get(node) || rawContent;
      const trimmedOriginal = original.trim();

      // Look up Arabic translation
      const translation = this.get(trimmedOriginal);
      if (translation && translation !== trimmedOriginal && trimmed !== translation) {
        const leading = original.match(/^\s*/)?.[0] || '';
        const trailing = original.match(/\s*$/)?.[0] || '';
        node.textContent = leading + translation + trailing;
      }
    } else {
      // ENGLISH MODE: Revert to English

      // 1. If originalTextMap has non-Arabic text, restore it immediately
      const original = this.originalTextMap.get(node);
      if (original && !/[\u0600-\u06FF]/.test(original)) {
        if (node.textContent !== original) {
          node.textContent = original;
        }
        return;
      }

      // 2. If node contains Arabic, perform reverse lookup in arabicToEnMap
      if (/[\u0600-\u06FF]/.test(trimmed)) {
        const enTranslation = this.getEnglish(trimmed);
        if (enTranslation && enTranslation !== trimmed) {
          const leading = rawContent.match(/^\s*/)?.[0] || '';
          const trailing = rawContent.match(/\s*$/)?.[0] || '';
          node.textContent = leading + enTranslation + trailing;
          // Cache the resolved English text as the original
          this.originalTextMap.set(node, node.textContent);
        }
      }
    }
  }

  private runFullTranslation(): void {
    if (typeof document === 'undefined') return;
    const root = document.querySelector('app-root') || document.body;
    if (root) {
      this.translateElement(root);
    }
  }

  private scheduleTranslation(): void {
    if (this.debounceTimer) {
      clearTimeout(this.debounceTimer);
    }
    this.debounceTimer = setTimeout(() => {
      this.runFullTranslation();
    }, 40);
  }

  private setupAutoDomTranslator(): void {
    if (typeof window === 'undefined') return;

    // React immediately when language signal changes
    effect(() => {
      const lang = this.state.lang();
      this.originalTextMap = new WeakMap<Node, string>();
      setTimeout(() => {
        this.runFullTranslation();
      }, 20);
    });

    // Listen to router navigation ends to translate newly navigated view
    if (this.router) {
      this.router.events.subscribe(event => {
        if (event instanceof NavigationEnd) {
          this.scheduleTranslation();
          setTimeout(() => this.scheduleTranslation(), 150);
        }
      });
    }

    // Observe DOM changes (including async @for lists, dynamic rows, and characterData)
    this.observer = new MutationObserver(mutations => {
      if (this.isTranslating) return;

      let hasRelevantChanges = false;
      for (const m of mutations) {
        if (m.type === 'childList' && (m.addedNodes.length > 0 || m.removedNodes.length > 0)) {
          hasRelevantChanges = true;
          break;
        }
        if (m.type === 'characterData') {
          hasRelevantChanges = true;
          break;
        }
      }

      if (hasRelevantChanges) {
        this.scheduleTranslation();
      }
    });

    const startObserving = () => {
      if (document.body && this.observer) {
        this.observer.observe(document.body, {
          childList: true,
          subtree: true,
          characterData: true
        });
      }
    };

    if (typeof document !== 'undefined') {
      if (document.readyState === 'loading') {
        window.addEventListener('DOMContentLoaded', startObserving);
      } else {
        startObserving();
      }
    }
  }
}
