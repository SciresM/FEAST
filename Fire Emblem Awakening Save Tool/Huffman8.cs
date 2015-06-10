/*
 * Mostly taken from huffman.c: http://www.romhacking.net/utilities/826/
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fire_Emblem_Awakening_Save_Tool
{
    public unsafe class Huffman8
    {
        const byte CMD_CODE = 0x28; // 8-bit Huffman magic number
        const uint HUF_LNODE = 0;
        const uint HUF_RNODE = 1;

        const int HUF_SHIFT = 1;
        const uint HUF_MASK = 0x80;
        const uint HUF_MASK4 = 0x80000000;

        const uint HUF_LCHAR = 0x80;
        const uint HUF_RCHAR = 0x40;
        const uint HUF_NEXT = 0x3F;

        const uint RAW_MINIM = 0x0;
        const uint RAW_MAXIM = 0xFFFFFF;

        const uint HUF_MINIM = 0x4;
        const uint HUF_MAXIM = 0x1400000;

        class HuffmanNode
        {
            public uint symbol;
            public uint weight;
            public uint leafs;

            public HuffmanNode dad;
            public HuffmanNode lson;
            public HuffmanNode rson;
        }

        class HuffmanCode
        {
            public uint nbits;
            public byte[] codework;
        }

        HuffmanNode[] tree;
        byte[] codetree, codemask;
        HuffmanCode[] codes;
        uint max_symbols = 0x100, num_leafs, num_nodes;
        uint[] freqs;
        int num_bits;

        public byte[] Decompress(byte[] data)
        {
            byte* pak;
            byte* raw;
            byte* pak_end;
            byte* raw_end;
            uint pak_len, raw_len, header;
            byte* tree;
            uint pos, next, mask4, code, ch;
            int nbits;

            pak_len = (uint)data.Length;

            fixed (byte* pak_buffer = &data[0])
            {
                header = *pak_buffer;
                num_bits = (int)(header & 0xF);
                raw_len = *(uint*)pak_buffer >> 8;
                byte[] raw_data = new byte[raw_len];
                fixed (byte* raw_buffer = &raw_data[0])
                {
                    pak = pak_buffer + 4;
                    raw = raw_buffer;
                    pak_end = pak_buffer + pak_len;
                    raw_end = raw_buffer + raw_len;

                    tree = pak;
                    pak += (*pak + 1) << 1;

                    nbits = 0;

                    pos = *(tree + 1);
                    next = 0;

                    mask4 = 0;

                    code = *(uint*)pak;

                    while (raw < raw_end)
                    {
                        if ((mask4 >>= HUF_SHIFT) == 0)
                        {
                            if (pak + 3 >= pak_end) break;
                            code = *(uint*)pak;
                            pak += 4;
                            mask4 = HUF_MASK4;
                        }

                        next += ((pos & HUF_NEXT) + 1) << 1;

                        if ((code & mask4) == 0)
                        {
                            ch = pos & HUF_LCHAR;
                            pos = *(tree + next);
                        }
                        else
                        {
                            ch = pos & HUF_RCHAR;
                            pos = *(tree + next + 1);
                        }

                        if (ch != 0)
                        {
                            *raw |= (byte)(pos << nbits);
                            ////  *raw = (*raw << num_bits) | pos; 
                            nbits = (nbits + num_bits) & 7;
                            if (nbits == 0) raw++;

                            pos = *(tree + 1);
                            next = 0;
                        }
                    }

                    raw_len = (uint)(raw - raw_buffer);
                    return raw_data;
                }
            }
        }

        public byte[] Compress(byte[] data)
        {
            byte* pak;
            byte* raw;
            byte* pak_end;
            byte* raw_end;
            byte* cod;
            uint raw_len, pak_len, len, new_len;
            HuffmanCode code;
            int cwork;
            byte mask;
            uint* pk4 = stackalloc uint[1];
            uint mask4, ch;
            int nbits;

            num_bits = 8;
            raw_len = (uint)data.Length;

            fixed (byte* raw_buffer = &data[0])
            {
                byte[] pbuf = new byte[HUF_MAXIM + 1];
                fixed (byte* pak_buffer = &pbuf[0])
                {
                    pak_len = 4 + (max_symbols << 1) + raw_len + 3;
                    *(uint*)pak_buffer = (CMD_CODE) | (raw_len << 8);
                    pak = pak_buffer + 4;
                    raw = raw_buffer;
                    raw_end = raw_buffer + raw_len;
                    HUF_InitFreqs();
                    HUF_CreateFreqs(raw_buffer, (int)raw_len);

                    HUF_InitTree();
                    HUF_CreateTree();

                    HUF_InitCodeTree();
                    HUF_CreateCodeTree();

                    HUF_InitCodeWorks();
                    HUF_CreateCodeWorks();

                    fixed (byte* cd = &codetree[0])
                    {
                        cod = cd;
                        len = (uint)((*cod + 1) << 1);
                        while (len-- != 0) *pak++ = *cod++;
                        mask4 = 0;
                        while (raw < raw_end)
                        {
                            ch = *raw++;


                            for (nbits = 8; nbits != 0; nbits -= num_bits)
                            {
                                code = codes[ch & ((1 << num_bits) - 1)];
                                ////  code = codes[ch >> (8 - num_bits)];

                                len = code.nbits;
                                cwork = 0;

                                mask = (byte)HUF_MASK;
                                while (len-- != 0)
                                {
                                    if ((mask4 >>= HUF_SHIFT) == 0)
                                    {
                                        mask4 = HUF_MASK4;
                                        *(pk4 = (uint*)pak) = 0;
                                        pak += 4;
                                    }
                                    if ((code.codework[cwork] & mask) != 0) *pk4 |= mask4;
                                    if ((mask >>= HUF_SHIFT) == 0)
                                    {
                                        mask = (byte)HUF_MASK;
                                        cwork++;
                                    }
                                }

                                ch >>= num_bits;
                                ////  ch = (ch << num_bits) & 0xFF;
                            }
                        }
                        pak_len = (uint)(pak - pak_buffer);
                        return pbuf.Take((int)pak_len).ToArray();
                    }
                }
            }
        }

        private void HUF_InitFreqs()
        {
            uint i;

            freqs = new uint[max_symbols];

            for (i = 0; i < max_symbols; i++) freqs[i] = 0;
        }

        private void HUF_CreateFreqs(byte* raw_buffer, int raw_len)
        {
            uint ch;
            int nbits;
            uint i;

            for (i = 0; i < raw_len; i++)
            {
                ch = *raw_buffer++;
                for (nbits = 8; nbits != 0; nbits -= num_bits)
                {
                    freqs[ch >> (8 - num_bits)]++;
                    ch = (ch << num_bits) & 0xFF;
                }
            }

            num_leafs = 0;
            for (i = 0; i < max_symbols; i++) if (freqs[i] != 0) num_leafs++;
            if (num_leafs < 2)
            {
                if (num_leafs == 1)
                {
                    for (i = 0; i < max_symbols; i++)
                    {
                        if (freqs[i] != 0)
                        {
                            freqs[i] = 1;
                            break;
                        }
                    }
                }
                while (num_leafs++ < 2)
                {
                    for (i = 0; i < max_symbols; i++)
                    {
                        if (freqs[i] == 0)
                        {
                            freqs[i] = 2;
                            break;
                        }
                    }
                }
            }
            num_nodes = (num_leafs << 1) - 1;
        }

        private void HUF_InitTree()
        {
            tree = new HuffmanNode[num_nodes];
            uint i;
            for (i = 0; i < num_nodes; i++) tree[i] = null;
        }

        private void HUF_CreateTree()
        {
            HuffmanNode lnode;
            HuffmanNode rnode;
            uint lweight, rweight, num_node;
            uint i;

            num_node = 0;
            for (i = 0; i < max_symbols; i++)
            {
                if (freqs[i] != 0)
                {
                    HuffmanNode node = new HuffmanNode();
                    tree[num_node++] = node;

                    node.symbol = i;
                    node.weight = freqs[i];
                    node.leafs = 1;
                    node.dad = null;
                    node.lson = null;
                    node.rson = null;
                }
            }


            while (num_node < num_nodes)
            {
                lnode = rnode = null;
                lweight = rweight = 0;

                for (i = 0; i < num_node; i++)
                {
                    if (tree[i].dad == null)
                    {
                        if (lweight == 0 || (tree[i].weight < lweight))
                        {
                            rweight = lweight;
                            rnode = lnode;
                            lweight = tree[i].weight;
                            lnode = tree[i];
                        }
                        else if (rweight == 0 || (tree[i].weight < rweight))
                        {
                            rweight = tree[i].weight;
                            rnode = tree[i];
                        }
                    }
                }

                HuffmanNode node = new HuffmanNode();
                tree[num_node++] = node;

                node.symbol = num_node - num_leafs + max_symbols;
                node.weight = lnode.weight + rnode.weight;
                node.leafs = lnode.leafs + rnode.leafs;
                node.dad = null;
                node.lson = lnode;
                node.rson = rnode;

                lnode.dad = rnode.dad = node;
            }
        }

        private void HUF_InitCodeTree()
        {
            uint max_nodes;
            uint i;

            max_nodes = (((num_leafs - 1) | 1) + 1) << 1;

            codetree = new byte[max_nodes];
            codemask = new byte[max_nodes];

            for (i = 0; i < max_nodes; i++)
            {
                codetree[i] = 0;
                codemask[i] = 0;
            }
        }

        private void HUF_CreateCodeTree()
        {
            uint i;

            i = 0;

            codetree[i] = (byte)((num_leafs - 1) | 1);
            codemask[i] = 0;

            HUF_CreateCodeBranch(tree[num_nodes - 1], i + 1, i + 2);
            HUF_UpdateCodeTree();

            i = (uint)((codetree[0] + 1) << 1);
            while (--i != 0) if (codemask[i] != 0xFF) codetree[i] |= codemask[i];
        }

        private uint HUF_CreateCodeBranch(HuffmanNode root, uint p, uint q)
        {
            HuffmanNode[] stack = new HuffmanNode[2 * root.leafs];
            uint r, s, mask;
            uint l_leafs, r_leafs;

            if (root.leafs <= HUF_NEXT + 1)
            {
                HuffmanNode node;
                s = r = 0;
                stack[r++] = root;

                while (s < r)
                {
                    if ((node = stack[s++]).leafs == 1)
                    {
                        if (s == 1) { codetree[p] = (byte)node.symbol; codemask[p] = 0xFF; }
                        else { codetree[q] = (byte)node.symbol; codemask[q++] = 0xFF; }
                    }
                    else
                    {
                        mask = 0;
                        if (node.lson.leafs == 1) mask |= HUF_LCHAR;
                        if (node.rson.leafs == 1) mask |= HUF_RCHAR;

                        if (s == 1) { codetree[p] = (byte)((r - s) >> 1); codemask[p] = (byte)mask; }
                        else { codetree[q] = (byte)((r - s) >> 1); codemask[q++] = (byte)mask; }

                        stack[r++] = node.lson;
                        stack[r++] = node.rson;
                    }
                }
            }
            else
            {
                mask = 0;
                if (root.lson.leafs == 1) mask |= HUF_LCHAR;
                if (root.rson.leafs == 1) mask |= HUF_RCHAR;

                codetree[p] = 0; codemask[p] = (byte)mask;

                if (root.lson.leafs <= root.rson.leafs)
                {
                    l_leafs = HUF_CreateCodeBranch(root.lson, q, q + 2);
                    r_leafs = HUF_CreateCodeBranch(root.rson, q + 1, q + (l_leafs << 1));
                    codetree[q + 1] = (byte)(l_leafs - 1);
                }
                else
                {
                    r_leafs = HUF_CreateCodeBranch(root.rson, q + 1, q + 2);
                    l_leafs = HUF_CreateCodeBranch(root.lson, q, q + (r_leafs << 1));
                    codetree[q] = (byte)(r_leafs - 1);
                }
            }

            return (root.leafs);
        }

        private void HUF_UpdateCodeTree()
        {
            uint max, inc, n0, n1, l0, l1, tmp0, tmp1;
            uint i, j, k;

            max = (uint)((codetree[0] + 1) << 1);
            for (i = 1; i < max; i++)
            {
                if ((codemask[i] != 0xFF) && (codetree[i] > HUF_NEXT))
                {
                    if ((i & 1) != 0 && (codetree[i - 1] == HUF_NEXT))
                    {
                        i--;
                        inc = 1;
                    }
                    else if ((i & 1) == 0 && (codetree[i + 1] == HUF_NEXT))
                    {
                        i++;
                        inc = 1;
                    }
                    else
                    {
                        inc = codetree[i] - HUF_NEXT;
                    }

                    n1 = (i >> 1) + 1 + codetree[i];
                    n0 = n1 - inc;

                    l1 = n1 << 1;
                    l0 = n0 << 1;

                    tmp0 = BitConverter.ToUInt16(codetree, (int)l1);
                    tmp1 = BitConverter.ToUInt16(codemask, (int)l1);
                    for (j = l1; j > l0; j -= 2)
                    {
                        Array.Copy(BitConverter.GetBytes(BitConverter.ToUInt16(codetree, (int)j - 2)), 0, codetree, (int)j, 2);
                        Array.Copy(BitConverter.GetBytes(BitConverter.ToUInt16(codemask, (int)j - 2)), 0, codemask, (int)j, 2);
                    }
                    Array.Copy(BitConverter.GetBytes(tmp0), 0, codetree, l0, 2);
                    Array.Copy(BitConverter.GetBytes(tmp1), 0, codemask, l0, 2);

                    codetree[i] -= (byte)inc;

                    for (j = i + 1; j < l0; j++)
                    {
                        if (codemask[j] != 0xFF)
                        {
                            k = (j >> 1) + 1 + codetree[j];
                            if ((k >= n0) && (k < n1)) codetree[j]++;
                        }
                    }

                    if (codemask[l0 + 0] != 0xFF) codetree[l0 + 0] += (byte)inc;
                    if (codemask[l0 + 1] != 0xFF) codetree[l0 + 1] += (byte)inc;

                    for (j = l0 + 2; j < l1 + 2; j++)
                    {
                        if (codemask[j] != 0xFF)
                        {
                            k = (j >> 1) + 1 + codetree[j];
                            if (k > n1) codetree[j]--;
                        }
                    }

                    i = (i | 1) - 2;
                }
            }
        }
        private void HUF_InitCodeWorks()
        {
            uint i;
            codes = new HuffmanCode[max_symbols];
            for (i = 0; i < max_symbols; i++) codes[i] = null;
        }

        private void HUF_CreateCodeWorks()
        {
            uint symbol, nbits, maxbytes, nbit;
            byte[] scode = new byte[100];
            byte mask;
            uint i, j;

            for (i = 0; i < num_leafs; i++)
            {
                HuffmanNode node;
                node = tree[i];
                symbol = node.symbol;

                nbits = 0;
                while (node.dad != null)
                {
                    scode[nbits++] = node.dad.lson == node ? (byte)HUF_LNODE : (byte)HUF_RNODE;
                    node = node.dad;
                }
                maxbytes = (nbits + 7) >> 3;

                HuffmanCode code = new HuffmanCode();

                codes[symbol] = code;
                code.nbits = nbits;
                code.codework = new byte[maxbytes];

                for (j = 0; j < maxbytes; j++) code.codework[j] = 0;

                mask = (byte)HUF_MASK;
                j = 0;
                for (nbit = nbits; nbit != 0; nbit--)
                {
                    if (scode[nbit - 1] != 0) code.codework[j] |= mask;
                    if ((mask >>= HUF_SHIFT) == 0)
                    {
                        mask = (byte)HUF_MASK;
                        j++;
                    }
                }
            }
        }
    }
}
